using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecordedObject : MonoBehaviour
{
    public ThingsToRecord RecordOptions = ThingsToRecord.Position;
    public bool UseFixedUpdate = true;
    public RecordingStatus status = RecordingStatus.None;
    private Rigidbody2D body2d;
    private Rigidbody body;


    public DataRecord<int> Timer = new DataRecord<int>();
    public DataRecord<Vector3> Position = new DataRecord<Vector3>();
    private DataRecord<Quaternion> Rotation = new DataRecord<Quaternion>();
    private DataRecord<Vector3> Scale = new DataRecord<Vector3>();
    private DataRecord<Vector3> Velocity = new DataRecord<Vector3>();
    private DataRecord<Vector2> Velocity2D = new DataRecord<Vector2>();
    private DataRecord<Vector3> AngVelocity = new DataRecord<Vector3>();
    private DataRecord<float> AngVelocity2D = new DataRecord<float>();

    private void Start()
    {
        body2d = GetComponent<Rigidbody2D>();
        body = GetComponent<Rigidbody>();
    }
    public void StartRecording()
    {
        if (body != null) body.isKinematic = false;
        if (body2d != null) body2d.simulated = true;
        if (gam != null) StopCoroutine(gam);
        Timer.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Position)) Position.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Scale)) Scale.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Rotation)) Rotation.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity)) Velocity.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity)) AngVelocity.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity2D)) Velocity2D.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity2D)) AngVelocity2D.StartRecording(Time.time);
        status = RecordingStatus.Recording;
    }
    public void StartPlayback(float spd)
    {
        if (body != null) body.isKinematic = true;
        if (body2d != null) body2d.simulated = false;
        Timer.StartPlayback(Time.time, spd);
        if (RecordOptions.HasFlag(ThingsToRecord.Position)) Position.StartPlayback(Time.time, spd);
        if (RecordOptions.HasFlag(ThingsToRecord.Scale)) Scale.StartPlayback(Time.time, spd);
        if (RecordOptions.HasFlag(ThingsToRecord.Rotation)) Rotation.StartPlayback(Time.time, spd);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity)) Velocity.StartPlayback(Time.time, spd);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity)) AngVelocity.StartPlayback(Time.time, spd);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity2D)) Velocity2D.StartPlayback(Time.time, spd);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity2D)) AngVelocity2D.StartPlayback(Time.time, spd);
        status = RecordingStatus.Playing;
        if (gam != null) StopCoroutine(gam);
        gam = StartCoroutine(Gamin());
    }

    public Action PollAll = null;

    public void StopRecording()
    {
        Timer.StopRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Position)) Position.StopRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Scale)) Scale.StopRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Rotation)) Rotation.StopRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity)) Velocity.StopRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity)) AngVelocity.StopRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity2D)) Velocity2D.StopRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity2D)) AngVelocity2D.StopRecording(Time.time);
        status = RecordingStatus.None;
    }

    public void StopPlayback()
    {
        status = RecordingStatus.None;
        if (body != null) body.isKinematic = false;
        if (body2d != null) body2d.simulated = true;
        if (gam != null) StopCoroutine(gam);
    }
    public void StopPlaybackAndPickupRecording()
    {
        if (status != RecordingStatus.Playing) throw new Exception("Pickup recording when playing back!");
        status = RecordingStatus.Recording;
        if (body != null) body.isKinematic = false;
        if (body2d != null) body2d.simulated = true;
        if (gam != null) StopCoroutine(gam);
        Timer.ResumeRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Position)) Position.ResumeRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Scale)) Scale.ResumeRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Rotation)) Rotation.ResumeRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity)) Velocity.ResumeRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity)) AngVelocity.ResumeRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity2D)) Velocity2D.ResumeRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity2D)) AngVelocity2D.ResumeRecording(Time.time);
    }
    public void Poll()
    {
        Timer.PollData(1, Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Position)) Position.PollData(transform.localPosition, Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Scale)) Scale.PollData(transform.localScale, Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Rotation)) Rotation.PollData(transform.localRotation, Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity)) Velocity.PollData(body.linearVelocity, Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity)) AngVelocity.PollData(body.angularVelocity, Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity2D)) Velocity2D.PollData(body2d.linearVelocity, Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity2D)) AngVelocity2D.PollData(body2d.angularVelocity, Time.time);
    }

    public void _Pause()
    {
        status = RecordingStatus.None;
        Timer.Pause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Position)) Position.Pause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Scale)) Scale.Pause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Rotation)) Rotation.Pause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity)) Velocity.Pause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity)) AngVelocity.Pause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity2D)) Velocity2D.Pause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity2D)) AngVelocity2D.Pause(Time.time);
    }

    public void _Unpause()
    {
        Timer.Unpause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Position)) Position.Unpause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Scale)) Scale.Unpause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Rotation)) Rotation.Unpause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity)) Velocity.Unpause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity)) AngVelocity.Unpause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity2D)) Velocity2D.Unpause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity2D)) AngVelocity2D.Unpause(Time.time);
    }

    public void _Unpause2()
    {
        Timer.Unpause(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Position)) Position.UnpauseWithLeap(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Scale)) Scale.UnpauseWithLeap(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Rotation)) Rotation.UnpauseWithLeap(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity)) Velocity.UnpauseWithLeap(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity)) AngVelocity.UnpauseWithLeap(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity2D)) Velocity2D.UnpauseWithLeap(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity2D)) AngVelocity2D.UnpauseWithLeap(Time.time);
    }

    public void UnpausePlayback()
    {
        if (body != null) body.isKinematic = true;
        if (body2d != null) body2d.simulated = false;
        _Unpause();
        status = RecordingStatus.Playing;
        if (gam != null) StopCoroutine(gam);
        gam = StartCoroutine(Gamin());
    }

    public void PausePlayback()
    {
        if (body != null) body.isKinematic = false;
        if (body2d != null) body2d.simulated = true;
        _Pause();
        if (gam != null) StopCoroutine(gam);
    }

    public void PauseRecording()
    {
        _Pause();
    }

    public void UnpauseRecording()
    {
        _Unpause2();
        status = RecordingStatus.Recording;
    }

    private void FixedUpdate()
    {
        if (UseFixedUpdate) PollAll = Poll;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (status == RecordingStatus.None)
            {
                StartRecording();
            }
            else if (status == RecordingStatus.Recording)
            {

                StopRecording();
            }
            else
            {
                Debug.LogWarning("Can not start recording while playing back!");
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartPlayback(1);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartPlayback(-1);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StopPlaybackAndPickupRecording();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (status == RecordingStatus.None)
            {
                UnpausePlayback();
            }
            else
            {
                PausePlayback();
            }
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (status == RecordingStatus.None)
            {
                UnpauseRecording();
            }
            else
            {
                PauseRecording();
            }
        }
        if (status == RecordingStatus.Recording)
        {
            if (UseFixedUpdate)
            {
                if (PollAll != null)
                {
                    PollAll();
                    PollAll = null;
                }
            }
            else
            {
                Poll();
            }
        }
        if (status == RecordingStatus.Playing)
        {
            if (RecordOptions.HasFlag(ThingsToRecord.Position))
            {
                var g = Position.PollPlayback(Time.time);
                if (g.HasValue)
                {
                    StartCoroutine(OXLerp.Linear((x) =>
                    {
                        transform.localPosition = Vector3.Lerp(g.Value.b, g.Value.c, x);
                    }, g.Value.a));
                }
            }
            if (RecordOptions.HasFlag(ThingsToRecord.Scale))
            {
                var g = Scale.PollPlayback(Time.time);
                if (g.HasValue)
                {
                    StartCoroutine(OXLerp.Linear((x) =>
                    {
                        transform.localScale = Vector3.Lerp(g.Value.b, g.Value.c, x);
                    }, g.Value.a));
                }
            }
            if (RecordOptions.HasFlag(ThingsToRecord.Rotation))
            {
                var g = Rotation.PollPlayback(Time.time);
                if (g.HasValue)
                {
                    StartCoroutine(OXLerp.Linear((x) =>
                    {
                        transform.localRotation = Quaternion.Slerp(g.Value.b, g.Value.c, x);
                    }, g.Value.a));
                }
            }
            if (RecordOptions.HasFlag(ThingsToRecord.Velocity))
            {
                var g = Velocity.PollPlayback(Time.time);
                if (g.HasValue)
                {
                    StartCoroutine(OXLerp.Linear((x) =>
                    {
                        body.linearVelocity = Vector3.Lerp(g.Value.b, g.Value.c, x);
                    }, g.Value.a));
                }
            }
            if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity))
            {
                var g = AngVelocity.PollPlayback(Time.time);
                if (g.HasValue)
                {
                    StartCoroutine(OXLerp.Linear((x) =>
                    {
                        body.angularVelocity = Vector3.Lerp(g.Value.b, g.Value.c, x);
                    }, g.Value.a));
                }
            }
            if (RecordOptions.HasFlag(ThingsToRecord.Velocity2D))
            {
                var g = Velocity2D.PollPlayback(Time.time);
                if (g.HasValue)
                {
                    StartCoroutine(OXLerp.Linear((x) =>
                    {
                        body2d.linearVelocity = Vector2.Lerp(g.Value.b, g.Value.c, x);
                    }, g.Value.a));
                }
            }
            if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity2D))
            {
                var g = AngVelocity2D.PollPlayback(Time.time);
                if (g.HasValue)
                {
                    StartCoroutine(OXLerp.Linear((x) =>
                    {
                        body2d.angularVelocity = Mathf.Lerp(g.Value.b, g.Value.c, x);
                    }, g.Value.a));
                }
            }
        }

    }
    public IEnumerator Gamin()
    {
        var d = Timer.record[1].a / Timer.playback_speed;
        yield return new WaitForSeconds(d);
        StopPlayback();
    }
    public Coroutine gam;

    [Flags]
    public enum ThingsToRecord
    {
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
        Velocity = 1 << 3,
        AngVelocity = 1 << 4,
        Velocity2D = 1 << 5,
        AngVelocity2D = 1 << 6,
    }
    public enum RecordingStatus
    {
        None,
        Recording,
        Playing,
    }

}
[System.Serializable]
public class DataRecord<T>
{
    public List<MultiRef<float, T>> record = new List<MultiRef<float, T>>();
    private float starttime = 0;
    private float pause_progress = 0;
    public void StartRecording(float t)
    {
        record.Clear();
        starttime = t;
    }
    public void StopRecording(float time)
    {
        time -= starttime;
        var dingle = record.Last();
        record.Add(new MultiRef<float, T>(time, dingle.b));
    }
    public void ResumeRecording(float time)
    {
        if (time == starttime) return;
        if (reversed)
        {
            throw new NotImplementedException();
        }
        else
        {
            int keeps = 0;
            float progress = time - starttime;

            for (int i = 0; i < record.Count; i++)
            {
                if (record[i].a < progress) keeps++;
            }
            if (keeps == 0) return;
            record = record.GetRange(0, keeps);
        }


    }
    public const float delta = 1 / 50f;
    public void PollData(T data, float time)
    {
        time -= starttime;
        if (record.Count == 0)
        {
            record.Add(new MultiRef<float, T>(time, data));
            return;
        }
        var dingle = record.Last();
        if (!data.Equals(dingle.b))
        {
            var dif = time - record.Last().a;
            if (dif > (delta + 0.001f))
            {
                record.Add(new MultiRef<float, T>(time - delta, dingle.b));
            }
            record.Add(new MultiRef<float, T>(time, data));
        }
    }
    private int last = 0;
    public float playback_speed = 1;
    private bool reversed = false;
    public T StartPlayback(float t, float speed = 1)
    {
        playback_speed = speed;

        last = 0;
        starttime = t;
        reversed = false;
        if (speed < 0)
        {
            return StartPlaybackReverse(t);
        }
        else if (speed == 0)
        {
            throw new Exception("Can not play record at zero speed lol");
        }
        return record[0].b;
    }
    private T StartPlaybackReverse(float t)
    {
        playback_speed *= -1;
        reversed = true;
        last = record.Count - 1;
        return record[record.Count - 1].b;
    }
    public MultiRef<float, T, T>? PollPlayback(float time)
    {
        if (reversed) return PollPlaybackReversed(time);
        time -= starttime;
        time *= playback_speed;
        if (last == record.Count - 1) return null;
        if (record[last].a <= time)
        {
            last++;
            var dif = record[last].a - record[last - 1].a;
            dif /= playback_speed;
            return new MultiRef<float, T, T>(dif, record[last - 1].b, record[last].b);
        }
        return null;
    }
    private MultiRef<float, T, T>? PollPlaybackReversed(float time)
    {
        var st = ((starttime - time) * playback_speed) + record[record.Count - 1].a;
        if (last == 0) return null;
        if (record[last].a >= st)
        {
            last--;
            var dif = Mathf.Abs(record[last + 1].a - record[last].a);
            dif /= playback_speed;
            return new MultiRef<float, T, T>(dif, record[last + 1].b, record[last].b);
        }
        return null;
    }

    public void Pause(float time)
    {
        pause_progress = time - starttime;
    }
    public void Unpause(float time)
    {
        starttime = time - pause_progress;
    }
    public void UnpauseWithLeap(float time)
    {
        starttime = time - pause_progress;

        var dingle = record.Last();

        time -= starttime;
        record.Add(new MultiRef<float, T>(time, dingle.b));
    }

}
