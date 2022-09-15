using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleFileBrowser;

public class FileBrowserHandler : MonoBehaviour
{
	public GameObject cell;
	private GameObject window;

	private void PositionWindow()
    {
		// Set the position of the file browser onto the wall UI
		window = GameObject.Find("SimpleFileBrowserCanvas(Clone)");
		window.transform.SetParent(gameObject.transform.parent, false);
		window.GetComponent<Canvas>().overrideSorting = true;
	}

	public void ShowLoadTimeSeriesFileBrowser()
	{
		// Show a select folder dialog
		FileBrowser.ShowLoadDialog(LoadTimeseries, Cancel, FileBrowser.PickMode.Folders, false, null, null, "Select folder to load data from", "Load");
		PositionWindow();
	}

	private void LoadTimeseries(string[] paths)
	{
		// Use the first path returned to load a timeseries
		if (paths.Length > 0)
			cell.GetComponent<MeshController>().LoadTimeseries(paths[0]);
	}

	public void ShowLoadAnnotationFileBrowser()
    {
		// Show a select dialog using .csv filter
		FileBrowser.SetFilters(false, ".csv");
		FileBrowser.SetDefaultFilter(".csv");
		FileBrowser.ShowLoadDialog(LoadAnnotation, Cancel, FileBrowser.PickMode.FilesAndFolders, false, null, null, "Select folder or .csv to load annotations from", "Load");
		PositionWindow();
	}

	public void LoadAnnotation(string[] paths)
	{
		// Load annotations using the first filepath returned
		if (paths.Length > 0)
			cell.GetComponent<MeshController>().LoadTimeseriesAnnotation(paths[0]);
	}

	public void ShowSaveAnnotationFileBrowser()
	{
		// Show a save dialog
		FileBrowser.ShowSaveDialog(SaveAnnotation, Cancel, FileBrowser.PickMode.Folders, false, null, null, "Select folder to save to", "Save");
		PositionWindow();
	}

	private void SaveAnnotation(string[] paths)
	{
		// Save annotations using the first filepath returned
		if (paths.Length > 0)
			cell.GetComponent<MeshController>().SaveAnnotations(paths[0]);
	}

	// Empty function purely for cancel
	private void Cancel() { }
}