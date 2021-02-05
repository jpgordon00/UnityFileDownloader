using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BeastBear {
  // mono-behavior for downloading files, just set 'PendingURLS' in editor
  // invoke 'Download' or set 'DownloadOnStart' to true
  // some fields are provided for editor-proxy access to GroupDownloader object
  public class GroupDownloaderComponent: MonoBehaviour {


    private GroupDownloader _downloader;

    public GroupDownloader Downloader {
      get {
        return _downloader;
      }
    }

    // true if 'Download' should be invoked upon 'Start'
    [SerializeField]
    private bool _downloadOnStart = true;

    [SerializeField]
    private string _downloadPath = Application.persistentDataPath;

    // true if the download handler should complete on failure
    [SerializeField] 
    private bool _abandonOnFailure = false;

    [SerializeField]
    private List<string> _pendingUrls = new List<string>();

    public List<string> PendingURLS {
        get {
            return _pendingUrls;
        }
    }

    // init GroupDownloader and invoke Download if enabled
    void Start() {
      _downloader = new GroupDownloader(this, PendingURLS);
      if (_downloadOnStart && _downloader != null) {
        _downloader.Download();
      }
    }

    // update downloader with internal values
    void Update() {
        if (_downloader == null) return;
        _downloader.DownloadPath = _downloadPath;
        _downloader.AbandonOnFailure = _abandonOnFailure;
    }
  }

}