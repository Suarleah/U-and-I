using System.Collections.Generic;
using UnityEngine;

public class NotebookManager : MonoBehaviour
{
    public GameObject page;
    public List<GameObject> pages = new List<GameObject>();
    public int pageIndex;
    public GameObject homePage;
    private Transform child;
    private Canvas myCanvas;
    void Awake()
    {
        child = GetComponent<Transform>();
        myCanvas = GetComponent<Canvas>();

        pageIndex = 0;

        CloseBook();

    }

    public void CreatePage(PatientSO patient) // call when patient is added
    {
        NotebookPage n = Instantiate(page, child).GetComponent<NotebookPage>();
        n.gameObject.SetActive(false);

        n.SetInfo(patient);
        pages.Add(n.gameObject);
    }

    public void AddInfoToPatient(PatientSO patient, string info)
    {
        foreach (GameObject go in pages)
        {
            NotebookPage np = go.GetComponent<NotebookPage>();
            if (np != null && np.patientSO == patient)
            {
                np.AddInfo(info);
                return;
            }
        }
    }

    public void OpenBook()
    {
        myCanvas.enabled = true;
    }
    public void CloseBook()
    {
        myCanvas.enabled = false;

        foreach (GameObject p in pages)
        {
            p.SetActive(false);
        }

        homePage.SetActive(true); // won't show up until myCanvas = true
    }

    public void CloseHomePage()
    {
        if (pages.Count == 0)
        {
            return;
        }

        pages[0].SetActive(true);
        homePage.SetActive(false);
    }

    public void FlipPageForward()
    { // 2 pages = 0, 1 on last page pageIndex = 2
        if (pageIndex + 1 >= pages.Count)
        {
            return;
        }

        if (pages[pageIndex + 1] == null)
        {
            return;
        }

        pageIndex += 1;

        pages[pageIndex].SetActive(true); // next page
        pages[pageIndex - 1].SetActive(false); // prior page


    }
    public void FlipPageBackward()
    { // 2 pages = 0, 1 on last page pageIndex = 2
        if (pageIndex == 0)
        {
            return;
        }

        if (pages[pageIndex - 1] == null)
        {
            return;
        }

        pageIndex -= 1;

        pages[pageIndex].SetActive(true); // next page
        pages[pageIndex + 1].SetActive(false); // prior page


    }

}
