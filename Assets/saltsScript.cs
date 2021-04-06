using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KMHelper;

public class saltsScript : MonoBehaviour {
    private int answer1 = -1, answer2 = -1;
    private int state = 0, count = 0;

    public KMSelectable button;
    private Coroutine waiting;

	// Use this for initialization
	void Start () {
        int sum = 0;
        foreach (int i in KMBombInfoExtensions.GetSerialNumberNumbers(GetComponent<KMBombInfo>()))
            sum += i;
        answer1 = sum / 5;
        answer2 = sum % 5;

        button.OnInteract += Press;
	}

    private bool Press()
    {
        GetComponent<KMSelectable>().AddInteractionPunch(0.1f);
        if (waiting != null)
            StopCoroutine(waiting);
        waiting = StartCoroutine(Wait());
        count++;
        return false;
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        if (state == 0)
        {
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
            if (count == answer1 + 1)
            {
                state = 1;
                count = 0;
            }
            else
            {
                GetComponent<KMBombModule>().HandleStrike();
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
            }
        }
        else if (state == 1)
        {
            if (count == answer2 + 1)
            {
                state = 2;

                GetComponent<KMBombModule>().HandlePass();
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);

                foreach (TriangleExplosion t in GetComponentsInChildren<TriangleExplosion>())
                    t.StartCoroutine(t.SplitMesh(false));
            }
            else
            {
                GetComponent<KMBombModule>().HandleStrike();
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
            }
        }

        count = 0;
        waiting = null;
    }
}
