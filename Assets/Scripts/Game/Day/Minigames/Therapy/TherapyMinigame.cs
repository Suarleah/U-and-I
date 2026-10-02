using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class TherapyMinigame : MinigameBase
{
    [Header("Lines")]
    public LineRenderer patientLine;
    public LineRenderer playerLine;
    public int pointCount = 5; // how many points make up each wave's curve
    public float displayWidth = 8f; // how wide the wave stretches in local space
    public float scrollSpeed = 2f; // how fast the wave scrolls sideways over time

    [Header("Patient Wave")]
    public float patientAmplitude;
    public float patientFrequency;
    public Vector2 amplitudeRange = new Vector2(0.5f, 3f); // min/max for random target
    public Vector2 frequencyRange = new Vector2(1f, 4f); // min/max for random target

    [Header("Things Player Touches")]
    public Slider amplitudeSlider;
    public Slider frequencySlider;
    public float playerAmplitude;
    public float playerFrequency;

    [Header("Difficulty")]
    public Image timerVisual;
    public float amplitudeTolerance = 0.2f; // how close counts as a match
    public float frequencyTolerance = 0.2f;//howdcdllekje
    public float timeGiven = 45f; // seconds allowed per round
    [SerializeField] private float scoreTime;

    public override void Open()
    {
        base.Open();

        //I need system for the Singles so it makes me do this :(
        patientAmplitude = UnityEngine.Random.Range(amplitudeRange.x, amplitudeRange.y); // roll a random target amplitude
        patientFrequency = UnityEngine.Random.Range(frequencyRange.x, frequencyRange.y); // roll a random target frequency

        playerAmplitude = amplitudeSlider.value; // will probs just be 0 anyways but whatever
        playerFrequency = frequencySlider.value;

        scoreTime = timeGiven;
    }

    public void OnAmplitudeChanged(Single value)
    {
        playerAmplitude = value; // update player amplitude whenever the slider moves
    }

    public void OnFrequencyChanged(Single value)
    {
        playerFrequency = value; // update player frequency whenever the slider moves
    }

    void Update()
    {
        float phase = Time.time * scrollSpeed; // constantly increasing becaus time count up :D

        PlotWave(patientLine, patientAmplitude, patientFrequency, phase); // redraw the target wave every frame
        PlotWave(playerLine, playerAmplitude, playerFrequency, phase); // redraw the player wave every frame

        scoreTime -= Time.deltaTime;
        timerVisual.fillAmount = scoreTime / timeGiven;

        if (scoreTime == 0)
        {
            CheckMatch();
        }
    }

    private void PlotWave(LineRenderer line, float amplitude, float frequency, float phase)
    {
        line.positionCount = pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1); // 0 to 1 normalized
            float screenX = t * displayWidth; // screen position 0 to ~1500
            float y = amplitude * Mathf.Sin((frequency * t * Mathf.PI * 2f) + phase); // frequency=cycles(s waves) visible on screen

            line.SetPosition(i, new Vector3(screenX, y, 0f));
        }
    }


    public void ResetSliders()
    {
        amplitudeSlider.value = 0;
        frequencySlider.value = 0;
    }
    public void CheckMatch()
    {
        bool ampMatch = Mathf.Abs(playerAmplitude - patientAmplitude) <= amplitudeTolerance; // check amplitude closeness
        // true if absolute value of diff between player and patient amp is less than or equal to the max/min diff amount
        // if they are closer than amplitudeTolerance together, they did it!!!
        bool freqMatch = Mathf.Abs(playerFrequency - patientFrequency) <= frequencyTolerance; // check frequency closeness

        if (ampMatch && freqMatch) // both need to be within tolerance to pass
        {
            Debug.Log("Close enough");
            

            if (scoreTime / timeGiven >= .80)
            {
                OnGameResult(6); // if they finished with 90% or more of their time left
                return;
            }
            if (scoreTime / timeGiven >= .70)
            {
                OnGameResult(5); // if they finished with 80% or more of their time left
                return;
            }
            if (scoreTime / timeGiven >= .60)
            {
                OnGameResult(4); // if they finished with 65% or more of their time left
                return;
            }
            if (scoreTime / timeGiven >= .50)
            {
                OnGameResult(3); // if they finished with 50% or more of their time left
                return;
            }
            if (scoreTime / timeGiven >= .40)
            {
                OnGameResult(2); // if they finished with 35% or more of their time left
                return;
            }
            if (scoreTime / timeGiven > 0)
            {
                OnGameResult(1); // if they finished
                return;
            }
            if (scoreTime == 0)
            {
                OnGameResult(0); // they didn't even finish
                return;
            }
        }
        else if (scoreTime == 0)
        {
            OnGameResult(0); // fail
        }
    }

    public void OnGameResult(int result)
    {
        Debug.Log(result);

        Finish(result);
    }
}