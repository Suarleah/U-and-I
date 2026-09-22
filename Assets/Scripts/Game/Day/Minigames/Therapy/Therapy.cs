using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Therapy : MinigameBase
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
    public float amplitudeTolerance = 0.2f; // how close counts as a match
    public float frequencyTolerance = 0.2f;//howdcdllekje
    public float timeGiven = 45f; // seconds allowed per round
    private float scoreTime;

    public override void Open()
    {
        base.Open();

        //I need system for the Singles so it makes me do this :(
        patientAmplitude = UnityEngine.Random.Range(amplitudeRange.x, amplitudeRange.y); // roll a random target amplitude
        patientFrequency = UnityEngine.Random.Range(frequencyRange.x, frequencyRange.y); // roll a random target frequency

        playerAmplitude = amplitudeSlider.value; // will probs just be 0 anyways but whatever
        playerFrequency = frequencySlider.value;

        scoreTime = timeGiven;
        Timer(timeGiven); // countdown
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

    }

    IEnumerator Timer(float t)
    {
        yield return new WaitForSecondsRealtime(1);
        scoreTime--;
        // I need to get the time it took them to do it
        if (scoreTime == 0)
        {
            CheckMatch();
            yield return null;
        }
        
    }


    /* TODO for plot wave
        Currently waves all All x values are 0 except for last point which is ~1500... Should be a number in the hundreds
        So, only first and last point get correct x value, y value seems to work fine...


        MAX/MIN Y  IS 400/-400 | top and bottom of the screen element
        MAX/MIN X IS 0 and 1500 | 0 is left edge because anchored to left and 1500 is right edge of screen element 
        screen element is an image (black box called screen), not the actual entire screen
        
        IN UNITY: 
        amplitude is 0-120 decibles
        frequency is the pitch and it is -3 to 3 where 1.0 is normal speed

        Maybe just ignore these values and fake the sound later....?

    */
    private void PlotWave(LineRenderer line, float amplitude, float frequency, float phase)
    {
        line.positionCount = pointCount; // make sure the line has enough points

        for (int i = 0; i < pointCount; i++) // e.g i = 1
        {
            
            float x = i * displayWidth; // stretch points across the display width

            float y = amplitude * Mathf.Sin((frequency * x) + phase);
            //Holy google

            line.SetPosition(i, new Vector3(x, y, 0f));
            //place point i at the x and y value that was just calculated by a formula on the internet
        }
    }

    public void CheckMatch()
    {
        bool ampMatch = Mathf.Abs(playerAmplitude - patientAmplitude) <= amplitudeTolerance; // check amplitude closeness
        // true if absolute value of diff between player and patient amp is less than or equal to the max/min diff amount
        // if they are closer than amplitudeTolerance together, they did it!!!
        bool freqMatch = Mathf.Abs(playerFrequency - patientFrequency) <= frequencyTolerance; // check frequency closeness

        if (ampMatch && freqMatch) // both need to be within tolerance to pass
        {
            float timeLeft = (timeGiven - scoreTime); // how much time is left
            if (timeLeft / timeGiven >= .90)
            {
                OnGameResult(6); // if they finished with 90% or more of their time left
            }
            if (timeLeft / timeGiven >= .80)
            {
                OnGameResult(5); // if they finished with 80% or more of their time left
            }
            if (timeLeft / timeGiven >= .65)
            {
                OnGameResult(4); // if they finished with 65% or more of their time left
            }
            if (timeLeft / timeGiven >= .50)
            {
                OnGameResult(3); // if they finished with 50% or more of their time left
            }
            if (timeLeft / timeGiven >= .35)
            {
                OnGameResult(2); // if they finished with 35% or more of their time left
            }
            if (timeLeft / timeGiven > 0)
            {
                OnGameResult(1); // if they finished
            }
            if (scoreTime == 0)
            {
                OnGameResult(0); // they didn't even finish
            }
        }
        else
        {
            OnGameResult(0); // fail
        }
    }

    public void OnGameResult(int result)
    {
        Finish(result);
    }
}