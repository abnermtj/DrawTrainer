using UnityEngine;
using System.Collections;
using System.IO;
using SimpleFileBrowser;
using UnityEngine.EventSystems;

public class fileExploreButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Brushes", ".abr"));
        FileBrowser.ShowLoadDialog((paths) => { Debug.Log("Selected: " + paths[0]); },
                                   () => { Debug.Log("Canceled"); },
                                   FileBrowser.PickMode.Files, false, null, null, "Select Folder", "Select"); // These default settings will be overwritten in the coroutine

        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load File", "Load");

        Debug.Log("File Selection status: " + FileBrowser.Success);
        if (!FileBrowser.Success)
        {
            yield break;
        }

        // Results of selection is in FileBrowser.Result[]
        for (int i = 0; i < FileBrowser.Result.Length; i++)
        {
            Debug.Log(FileBrowser.Result[i]);
        }

        //// Contrary to File.ReadAllBytes, this function works on Android 10+, as well
        //byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

        //// Or, copy the first file to persistentDataPath
        //string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
        //FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
    }
}