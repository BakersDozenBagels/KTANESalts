using System.Collections;
using UnityEngine;
using KMHelper;
using System.Text.RegularExpressions;

public class saltsScript : MonoBehaviour
{
    private int answer1 = -1, answer2 = -1;
    private int state = 0, count = 0;

    public KMSelectable button;
    public Texture blan;
    public Renderer ghost;
    private Coroutine waiting;

    private int _id = ++_idc;
    private static int _idc;

    void Start()
    {
        int sum = 0;
        foreach(int i in KMBombInfoExtensions.GetSerialNumberNumbers(GetComponent<KMBombInfo>()))
            sum += i;
        answer1 = sum / 5;
        answer2 = sum % 5;

        Debug.LogFormat("[Salts #{0}] Tap the ghost {1} times, then {2} times.", _id, answer1, answer2);

        button.OnInteract += Press;
    }

    private bool Press()
    {
        GetComponent<KMSelectable>().AddInteractionPunch(0.1f);
        if(waiting != null)
            StopCoroutine(waiting);
        waiting = StartCoroutine(Wait());
        count++;
        return false;
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        if(state == 0)
        {
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
            if(count == answer1 + 1)
            {
                state = 1;
                Debug.LogFormat("[Salts #{0}] Correct first set of taps.", _id);
            }
            else
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Salts #{0}] You tapped {1} times. Strike!", _id, count);
            }
        }
        else if(state == 1)
        {
            if(count == answer2 + 1)
            {
                state = 2;

                Debug.LogFormat("[Salts #{0}] Correct second set of taps. Solved!", _id);

                GetComponent<KMBombModule>().HandlePass();
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);

                foreach(TriangleExplosion t in GetComponentsInChildren<TriangleExplosion>())
                    t.StartCoroutine(t.SplitMesh(false));
            }
            else
            {
                Debug.LogFormat("[Salts #{0}] You tapped {1} times. Strike!", _id, count);

                GetComponent<KMBombModule>().HandleStrike();
            }
        }

        count = 0;
        waiting = null;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"""!{0} tap 3"" taps the module 3 times.";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        Match m;
        if((m = Regex.Match(command, @"^\s*blan\s*jumpscare\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
            ghost.material.mainTexture = blan;
        if((m = Regex.Match(command, @"^\s*(?:(?:press|tap|push|ghost|go)\s*)?(\d\d?)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            yield return null;
            int c = int.Parse(m.Groups[1].Value);
            for(int i = 0; i < c; i++)
            {
                button.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            yield return "strike";
            yield return "solve";
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while(waiting != null)
            yield return true;
        yield return null;
        if(state == 0)
        {
            for(int i = 0; i < answer1 + 1; i++)
            {
                button.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
        while(waiting != null)
            yield return true;
        yield return null;
        if(state == 1)
        {
            for(int i = 0; i < answer2 + 1; i++)
            {
                button.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
