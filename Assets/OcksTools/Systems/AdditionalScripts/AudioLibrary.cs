using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Scriptable Objects/OcksTools/AudioLibrary")]
public class AudioLibrary : ScriptableObject
{
    public List<OXSoundData> Datas = new();
    public List<OXMixerData> Mixers = new();
}
