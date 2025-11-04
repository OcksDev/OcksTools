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
        if(RecordOptions.HasFlag(ThingsToRecord.Position)) Position.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Scale)) Scale.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Rotation)) Rotation.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity)) Velocity.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity)) AngVelocity.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity2D)) Velocity2D.StartRecording(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity2D)) AngVelocity2D.StartRecording(Time.time);
        status = RecordingStatus.Recording;
    }
    public void StartPlayback()
    {
        if (body != null) body.isKinematic = true;
        if (body2d != null) body2d.simulated = false;
        Timer.StartPlayback(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Position)) Position.StartPlayback(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Scale)) Scale.StartPlayback(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Rotation)) Rotation.StartPlayback(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity)) Velocity.StartPlayback(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity)) AngVelocity.StartPlayback(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.Velocity2D)) Velocity2D.StartPlayback(Time.time);
        if (RecordOptions.HasFlag(ThingsToRecord.AngVelocity2D)) AngVelocity2D.StartPlayback(Time.time);
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
        status = 0;
        if (body != null) body.isKinematic = false;
        if (body2d != null) body2d.simulated = true;
        if(gam != null) StopCoroutine(gam);
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
    private void FixedUpdate()
    {
        if(UseFixedUpdate) PollAll = Poll;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartRecording();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StopRecording();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartPlayback();
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
        if(status == RecordingStatus.Playing)
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
        yield return new WaitForSeconds(Timer.record[1].a);
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
    float starttime = 0;    
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
    public const float delta = 1 / 50f;
    public void PollData(T data, float time)
    {
        time -= starttime;
        if(record.Count == 0)
        {
            record.Add(new MultiRef<float, T>(time, data));
            return;
        }
        var dingle = record.Last();
        if (!data.Equals(dingle.b))
        {
            var dif = time- record.Last().a;
            if(dif > (delta+0.001f))
            {
                record.Add(new MultiRef<float, T>(time-delta, dingle.b));
            }
            record.Add(new MultiRef<float, T>(time, data));
        }
    }
    int last = 0;
    public T StartPlayback(float t)
    {
        last = 0;
        starttime = t;
        return record[0].b;
    }
    public MultiRef<float,T,T>? PollPlayback(float time)
    {
        time -= starttime;
        if (last == record.Count - 1) return null;
        if (record[last].a <= time)
        {
            last++;
            var dif = record[last].a - record[last-1].a;
            return new MultiRef<float, T, T>(dif,record[last-1].b, record[last].b);
        }
        return null;
    }
}
