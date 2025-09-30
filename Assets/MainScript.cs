using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class MainScript : MonoBehaviour
{
    public VideoPlayer WallVideoPlayer;
    public VideoPlayer FloorVideoPlayer;
    public VideoPlayer TopVideoPlayer;
    public GameObject SparkPrefab;

    string currentdirect;
    string videopath;
    string[] videocontents;
    int currentvideoindex = 0;
    bool isPlaying = true;
    void Start()
    {
        currentdirect = Directory.GetCurrentDirectory();
        videopath = Path.Combine(currentdirect, "Videos");

        if (!Directory.Exists(videopath)) Directory.CreateDirectory(videopath);

        videocontents = Directory.GetDirectories(videopath);

        if (videocontents.Length == 0)
        {
            Debug.LogError("No video folders found in Videos directory. Baliw ka talaga");
            return;
        }

        // hide yung mouse cursor
        Cursor.visible = false;
        
        StartCoroutine(PlayVideos());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PreviousVideo();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            NextVideo();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            isPlaying = !isPlaying;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {

            WallVideoPlayer.Stop();
            FloorVideoPlayer.Stop();
            TopVideoPlayer.Stop();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!isPlaying) return;

            Vector3 mousepos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            mousepos.z = 0; // set z to 0 para makita sa 2D space

            GameObject spark = Instantiate(SparkPrefab, mousepos, Quaternion.identity);

            ParticleSystem ps = spark.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                Destroy(spark, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(spark, 2f);
            }
        }
    }

    IEnumerator PlayVideos()
    {
        string indexvideo = videocontents[currentvideoindex];

        string wallvidpath = Path.Combine(indexvideo, "wall.mp4");
        string floorvidpath = Path.Combine(indexvideo, "floor.mp4");
        string topvidpath = Path.Combine(indexvideo, "top.mp4");

        if (!File.Exists(wallvidpath) || !File.Exists(floorvidpath) || !File.Exists(topvidpath))
        {
            Debug.LogError($"One or more video files are missing in folder: {indexvideo}");
            yield break;
        }
        else
        {
            WallVideoPlayer.url = wallvidpath;
            FloorVideoPlayer.url = floorvidpath;
            TopVideoPlayer.url = topvidpath;
        }

        if (WallVideoPlayer.isPlaying)
        {
            WallVideoPlayer.Stop();
            FloorVideoPlayer.Stop();
            TopVideoPlayer.Stop();
        }

        // preparation ng video players para sabay mag play
        WallVideoPlayer.Prepare();
        FloorVideoPlayer.Prepare();
        TopVideoPlayer.Prepare();

        while (!WallVideoPlayer.isPrepared || !FloorVideoPlayer.isPrepared || !TopVideoPlayer.isPrepared)
        {
            yield return null;
        }

        WallVideoPlayer.Play();
        FloorVideoPlayer.Play();
        TopVideoPlayer.Play();
    }

    public void NextVideo()
    {
        currentvideoindex++;
        if (currentvideoindex >=videocontents.Length) currentvideoindex = 0;

        StartCoroutine(PlayVideos());
    }

    public void PreviousVideo()
    {
        currentvideoindex--;
        if (currentvideoindex < 0) currentvideoindex = videocontents.Length - 1;

        StartCoroutine(PlayVideos());
    }
}
