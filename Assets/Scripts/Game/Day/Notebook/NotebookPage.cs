using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotebookPage : MonoBehaviour
{
    [SerializeField] private Image patientImage;
    [SerializeField] private TextMeshProUGUI patientName;
    [SerializeField] private TextMeshProUGUI patientDesc;
     [SerializeField] private Transform infoHolder;
    [SerializeField] private GameObject infoText;

    public PatientSO patientSO;

    private NotebookManager notebookManager;
    void Start()
    {
        notebookManager = GetComponentInParent<NotebookManager>();
    }

    public void SetInfo(PatientSO patient)
    {
        patientSO = patient;
        patientImage.sprite = patient.myPhoto;
        patientName.text = patient.namee;
        patientDesc.text = patient.desc;
        
    }

    public void AddInfo(string info)
    {
        TextMeshProUGUI text = Instantiate(infoText, infoHolder).GetComponent<TextMeshProUGUI>();
        text.text = info;
    }

    public void Next()
    {
        notebookManager.FlipPageForward();
    }
    public void Back()
    {
        notebookManager.FlipPageBackward();
    }

    public void Exit()
    {
        notebookManager.CloseBook();
    }
}
