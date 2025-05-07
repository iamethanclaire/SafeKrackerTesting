using System.Collections.Generic;
using UnityEngine;

public class CursedQuestions : MonoBehaviour
{
    public static readonly List<string> CursedQuestionPool = new List<string>
    {
        "Is this number the meaning to life?",
        "Is this number the key to helping you escape?",
        "Does this number reveal the truth?",
        "Is this number cursed?",
        "Is this number the only way out?",
        "Will this number free you from the safe?",
        "Does this number reveal who trapped you here?",
        "Is this number meant to be forgotten?",
        "Does this number mean the end?",
        "Have you been here before?"
    };

    public static readonly List<string> WarningResponses = new List<string>
    {
        "You can feel the safe coldly staring back at you",
        "You feel a chill wrap around your spine, like a noose tightening.",
        "A whisper echoes: 'Do not ask what you do not wish to understand.'",
        "A force pulls your hands away. The safe doesn't like that.",
        "The air turns thick. It's harder to breathe.",
        "The safe hisses with disdain. A mistake, surely.",
        "The darkness deepens. Something isn't right.",
        "The safe refuses to respond. A warning left unspoken.",
        "A shadow moves where there should be none.",
        "You feel as if something is watching... waiting."
    };
}
