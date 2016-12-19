using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using MovementEffects;

namespace CutscenePlayback
{

    internal struct CutsceneOp
    {
        /// <summary>
        /// Nasty hack to create something that will automatically return as a finished coroutine, simplifying control flow by letting WaitUntilDone handle EVTCall ops.
        /// </summary>
        /// <returns></returns>
        static IEnumerator<float> dummyCoroutine ()
        {
            yield break;
        }

        Action<object, object> EVTCall; // immediately-executable things
        Func<object, object, IEnumerator<float>> FXCall; // coroutines
        object arg0;
        object arg1;

        public CutsceneOp(Action<object, object> evt, object _arg0 = null, object _arg1 = null)
        {
            FXCall = null;
            EVTCall = evt;
            arg0 = _arg0;
            arg1 = _arg1;
        }

        public CutsceneOp(Func<object, object, IEnumerator<float>> fx, object _arg0 = null, object _arg1 = null)
        {
            FXCall = fx;
            EVTCall = null;
            arg0 = _arg0;
            arg1 = _arg1;
        }

        public IEnumerator<float> Run ()
        {
            IEnumerator<float> r;
            if (EVTCall == null) r = Timing.RunCoroutine(FXCall(arg0, arg1));
            else
            {
                EVTCall(arg0, arg1);
                r = Timing.RunCoroutine(dummyCoroutine());
            }
            return r;
        }

        public IEnumerator<float> Run (string tag)
        {
            IEnumerator<float> r;
            if (EVTCall == null) r = Timing.RunCoroutine(FXCall(arg0, arg1), tag);
            else
            {
                EVTCall(arg0, arg1);
                r = Timing.RunCoroutine(dummyCoroutine());
            }
            return r;
        }
    }

    internal static class CutsceneDefs
    {
        public static CutsceneOp[] cutsceneEnding;
    }

    public class CutscenePlayer : MonoBehaviour
    {
        public AudioSource bgmSource;
        public AudioSource sfxSource;
        public Image backgroundImage;
        public Image foregroundImage;
        public Text textbox;
        public string myTag = "cutscenePlayer";
        private AsyncOperation currentOp;
        private Vector3 bgImgPos;
        private Vector3 fgImgPos;
        private CutsceneOp[] cutsceneOps;
        private bool running;
        private int opIndex;

        void Awake()
        {
            bgImgPos = backgroundImage.transform.position;
            fgImgPos = foregroundImage.transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (running)
            {
                if (currentOp.isDone && opIndex < cutsceneOps.Length)
                {
                    AsyncOperation op = (AsyncOperation)cutsceneOps[opIndex].Run(myTag);
                    opIndex++;
                }
            }
        }

        void cutsceneEVT_PlayMFX (object mfxClip, object notUsed1)
        {
            bgmSource.PlayOneShot((AudioClip)mfxClip);
        }

        void cutsceneEVT_PlaySFX (object sfxClip, object notUsed1)
        {
            sfxSource.PlayOneShot((AudioClip)sfxClip);
        }

        void cutsceneEVT_SetBGM (object bgmClip, object notUsed1)
        {
            bgmSource.clip = (AudioClip)bgmClip;
        }

        void cutsceneEVT_SetBackground(object sprite, object color = null)
        {
            backgroundImage.sprite = (Sprite)sprite;
            if (color != null) backgroundImage.color = (Color)color;
            else backgroundImage.color = Color.white;
            backgroundImage.transform.position = bgImgPos;
        }

        void cutsceneEVT_SetForeground(object sprite, object color = null)
        {
            foregroundImage.sprite = (Sprite)sprite;
            if (color != null) foregroundImage.color = (Color)color;
            else foregroundImage.color = Color.white;
            foregroundImage.transform.position = fgImgPos;
        }

        void cutsceneEVT_RefreshTextboxWith(object textString, object notUsed1)
        {
            textbox.text = (string)textString;
        }

        IEnumerator<float> cutsceneFX_scrollBackground(object direction, object dist_time_array) // this kinda sucks - dist_time_array needs to be a two-element float[] where element 0 = total distance and element 1 = total scroll time. eww. but it has to be object for the generic func...
        {
            Vector3 v = Vector3.zero;
            float distance = ((float[])dist_time_array)[0];
            float time = ((float[])dist_time_array)[1];
            switch ((Direction)direction)
            {
                case Direction.Down:
                    v = Vector3.down;
                    break;
                case Direction.DownLeft:
                    v = (Vector3.down + Vector3.left) / 2;
                    break;
                case Direction.DownRight:
                    v = (Vector3.down + Vector3.right) / 2;
                    break;
                case Direction.Up:
                    v = Vector3.up;
                    break;
                case Direction.UpLeft:
                    v = (Vector3.up + Vector3.left) / 2;
                    break;
                case Direction.UpRight:
                    v = (Vector3.up + Vector3.right) / 2;
                    break;
                case Direction.Left:
                    v = Vector3.left;
                    break;
                case Direction.Right:
                    v = Vector3.right;
                    break;
            }
            v *= distance;
            v += bgImgPos;
            float timer = 0;
            while (timer < time)
            {
                timer += Time.deltaTime;
                backgroundImage.transform.position = Vector3.Lerp(bgImgPos, v, time / timer);
                yield return 0f;
            }   
        }

        IEnumerator<float> cutsceneFX_scrollForeground(object direction, object dist_time_array) // this kinda sucks - dist_time_array needs to be a two-element float[] where element 0 = total distance and element 1 = total scroll time. eww. but it has to be object for the generic func...
        {
            Vector3 v = Vector3.zero;
            float distance = ((float[])dist_time_array)[0];
            float time = ((float[])dist_time_array)[1];
            switch ((Direction)direction)
            {
                case Direction.Down:
                    v = Vector3.down;
                    break;
                case Direction.DownLeft:
                    v = (Vector3.down + Vector3.left) / 2;
                    break;
                case Direction.DownRight:
                    v = (Vector3.down + Vector3.right) / 2;
                    break;
                case Direction.Up:
                    v = Vector3.up;
                    break;
                case Direction.UpLeft:
                    v = (Vector3.up + Vector3.left) / 2;
                    break;
                case Direction.UpRight:
                    v = (Vector3.up + Vector3.right) / 2;
                    break;
                case Direction.Left:
                    v = Vector3.left;
                    break;
                case Direction.Right:
                    v = Vector3.right;
                    break;
            }
            v *= distance;
            v += fgImgPos;
            float timer = 0;
            while (timer < time)
            {
                timer += Time.deltaTime;
                foregroundImage.transform.position = Vector3.Lerp(fgImgPos, v, time / timer);
                yield return 0f;
            }
        }

        IEnumerator<float> cutsceneFX_fadeOutBackground(object notUsed0, object notUsed1)
        {
            Color baseColor = backgroundImage.color;
            float timer = 0f;
            while (backgroundImage.color != Color.clear)
            {
                timer += Time.deltaTime;
                backgroundImage.color = Color.Lerp(baseColor, Color.clear, timer);
                yield return 0f;
            }
        }

        IEnumerator<float> cutsceneFX_fadeOutForeground(object notUsed0, object notUsed1)
        {
            Color baseColor = foregroundImage.color;
            float timer = 0f;
            while (foregroundImage.color != Color.clear)
            {
                timer += Time.deltaTime;
                foregroundImage.color = Color.Lerp(baseColor, Color.clear, timer);
                yield return 0f;
            }
        }
    }

}

