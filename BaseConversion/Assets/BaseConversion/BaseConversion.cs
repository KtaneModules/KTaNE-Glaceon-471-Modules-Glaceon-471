using System;
using System.Collections.Generic;
using UnityEngine;
using static KMSoundOverride;
using Random = UnityEngine.Random;

public class BaseConversion : MonoBehaviour
{
	public KMBombModule Module;
	public KMBombInfo Info;
	public KMAudio Audio;
	public KMModSettings Settings;

	public TextMesh BaseDisplayText;
	public TextMesh BoeforValueDisplayText;
	public TextMesh AfterValueDisplayText;
    public KMSelectable AnswerButton;
    public KMSelectable[] ButtonData;
    public uint Value;
	public uint BoeforBase;
	public uint AfterBase;
    public string AnswerValue;

    public static readonly string[] IntData = new string[]
	{
        "0",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
        "A",
        "B",
        "C",
        "D",
        "E",
        "F",
        "G",
        "H",
        "I",
        "J",
        "K",
        "L",
        "M",
        "N",
        "O",
        "P",
        "Q",
        "R",
        "S",
        "T",
        "U",
        "V",
        "W",
        "X",
        "Y",
        "Z",
    };
    public static readonly KeyCode[] KeyData = new KeyCode[]
    {
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
        KeyCode.A,
        KeyCode.B,
        KeyCode.C,
        KeyCode.D,
        KeyCode.E,
        KeyCode.F,
        KeyCode.G,
        KeyCode.H,
        KeyCode.I,
        KeyCode.J,
        KeyCode.K,
        KeyCode.L,
        KeyCode.M,
        KeyCode.N,
        KeyCode.O,
        KeyCode.P,
        KeyCode.Q,
        KeyCode.R,
        KeyCode.S,
        KeyCode.T,
        KeyCode.U,
        KeyCode.V,
        KeyCode.W,
        KeyCode.X,
        KeyCode.Y,
        KeyCode.Z,
    };

    public bool Activated;
    public bool ModuleSelected;
    public bool Solved;

    public static int ModuleIdCounter = 1;
    public int ModuleId;

    public void Start()
	{
		ModuleId = ModuleIdCounter++;
        Module.OnActivate += delegate () { Activated = true; };
        KMSelectable main = GetComponent<KMSelectable>();
        main.OnFocus += delegate () { ModuleSelected = true; };
        main.OnDefocus += delegate () { ModuleSelected = false; };

        AnswerButton.OnInteract += delegate ()
        {
            if (!Activated || Solved) return false;
            DebugLog("Answer : {0}", AfterValueDisplayText.text);
            Audio.PlayGameSoundAtTransform(SoundEffect.BigButtonPress, transform);
            if (AfterValueDisplayText.text == AnswerValue)
            {
                Module.HandlePass();
                Solved = true;
                DebugLog("Module solved");
            }
            else
            {
                Module.HandleStrike();
                AfterValueDisplayText.text = "";
            }
            return false;
        };
        foreach (KMSelectable button in ButtonData)
        {
            button.OnInteract += delegate ()
            {
                button.AddInteractionPunch(0.25f);
                Audio.PlayGameSoundAtTransform(SoundEffect.ButtonPress, transform);
                if (!ModuleSelected || Solved) return false;
                if (AfterValueDisplayText.text.Length >= 32) return false;
                AfterValueDisplayText.text += button.transform.Find("KeyText").GetComponent<TextMesh>().text.ToUpper();
                if (!AnswerValue.StartsWith(AfterValueDisplayText.text))
                {
                    Module.HandleStrike();
                    AfterValueDisplayText.text = "";
                }
                return false;
            };
        }

        Value = GetRandom();
        List<uint> bb = Settings.GetBoeforBaseNumbers(), ab = Settings.GetAfterBaseNumbers();
        if (bb.Count < ab.Count)
        {
            BoeforBase = bb.GetRandom();
            ab.RemoveAll(x => x == BoeforBase);
            if (ab.Count <= 0)
            {
                Module.HandlePass();
                BaseDisplayText.text = $"{BoeforBase} → {BoeforBase}";
                BoeforValueDisplayText.text = "It was too easy.";
                Solved = true;
                DebugLog("Module solved");
                return;
            }
            AfterBase = ab.GetRandom();
        }
        else
        {
            AfterBase = ab.GetRandom();
            bb.RemoveAll(x => x == AfterBase);
            if (bb.Count <= 0)
            {
                Module.HandlePass();
                BaseDisplayText.text = $"{AfterBase} → {AfterBase}";
                BoeforValueDisplayText.text = "It was too easy.";
                Solved = true;
                DebugLog("Module solved");
                return;
            }
            BoeforBase = bb.GetRandom();
        }
		BaseDisplayText.text = $"{BoeforBase} → {AfterBase}";
        
		if (BoeforBase == 10) BoeforValueDisplayText.text = Value.ToString();
		else
		{
			List<string> data = new List<string>();
			uint num = Value;
			while (true)
			{
				if (num % BoeforBase == num)
				{
                    data.Add(IntData[num]);
                    break;
				}
                uint remainder;
                num = DivRem(num, BoeforBase, out remainder);
                data.Add(IntData[remainder]);
            }
            data.Reverse();
            BoeforValueDisplayText.text = string.Join("", data.ToArray());
        }
        if (AfterBase == 10) AnswerValue = Value.ToString();
        else
        {
            List<string> data = new List<string>();
            uint num = Value;
            while (true)
            {
                if (num % AfterBase == num)
                {
                    data.Add(IntData[num]);
                    break;
                }
                uint remainder;
                num = DivRem(num, AfterBase, out remainder);
                data.Add(IntData[remainder]);
            }
            data.Reverse();
            AnswerValue = string.Join("", data.ToArray());
        }

        DebugLog("Base 10 Value : {0}", Value);
        DebugLog("BoeforBase : {0}", BoeforBase);
        DebugLog("AfterBase : {0}", AfterBase);
        DebugLog("Base {0} Value : {1}", BoeforBase, BoeforValueDisplayText.text);
        DebugLog("Base {0} Value : {1}", AfterBase, AnswerValue);
    }
	
	public void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter)) AnswerButton.OnInteract();
        for (int i = 0; i < KeyData.Length; i++)
        {
            if (!Input.GetKeyDown(KeyData[i])) continue;
            ButtonData[i].OnInteract();
            break;
        }

        float t = (BoeforValueDisplayText.text.Length - 12) / 20f;
        BoeforValueDisplayText.transform.localScale = new Vector3(Mathf.Lerp(0.05f, 0.025f, t), Mathf.Lerp(0.25f, 0.15f, t), 0.1f);

        t = (AfterValueDisplayText.text.Length - 12) / 20f;
        AfterValueDisplayText.transform.localScale = new Vector3(Mathf.Lerp(0.05f, 0.025f, t), Mathf.Lerp(0.25f, 0.15f, t), 0.1f);
    }

    public uint GetRandom()
	{
		uint min = Settings.GetMinimumValue(), max = Settings.GetMaximumValue();
		return (uint)Math.Floor(Random.value * (max - min)) + min;
	}

    public static uint DivRem(uint a, uint b, out uint result)
    {
        result = ((a % b) + b) % b;
        return a / b;
    }

    public void DebugLog(string log, params object[] args)
    {
        var data = string.Format(log, args);
        Debug.LogFormat("[Base Conversion #{0}] {1}", ModuleId, data);
    }
}
