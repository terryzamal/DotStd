﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotStd
{
    public class JobAttribute : Attribute
    {
        // Declare this as some job/task i want to execute from external source. checked in Compile.
        // Attach some attribute to the job/task so i can know more about it. Must accompany IJobWorker.

        public string Name;
        public List<string> Aliases;
    }

    public class JobState
    {
        // Last/Current run state of the job. Produce a JobWorker to run.
        // keep this in persistent storage.
        // leave scheduling defintion out

        public int Id { get; set; }   // PK in some persistent storage.

        public int RunningAppId { get; set; } = ValidState.kInvalidId;  // 0 = not running. else its currently running. (as far as we know on ConfigApp.AppId);

        // NOTE: LastRun can be set into the future to delay start.
        public DateTime LastRun { get; set; }       // last UTC start time when we tried to run this. or rety this. It might have failed or succeeded.

        public string LastResult { get; set; }      // what happened at/after LastRun? null = never run, "" = success or error description.
        public DateTime LastSuccess { get; set; }   // The last UTC start time we ran this and it succeeded. LastResult == ""

        public bool IsRunning
        {
            get
            {
                // Never run the same job reentramtly. always wait for the first to finish or figure out why its failng.
                return ValidState.IsValidId(RunningAppId);
            }
        }

        public JobState()
        { }

        public JobState(int id)
        {
            Id = id;
        }
    }

    public interface IJobWorker
    {
        // abstraction for some job/task i want to execute. exposed by assembly.
        // Code must expose this interface so i can call it externally.

        JobState State { get; set; }

        Task ExecuteAsync(string args);
    }

    public abstract class JobWorker : IJobWorker
    {
        // Can be used with JobTracker ? JobTypeName ?

        public JobState State { get; set; }

        public abstract Task ExecuteAsync(string args);

        public JobWorker(JobState def)
        {
            State = def;
        }
    }
}
