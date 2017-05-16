#load "..\SendAdoptionListEmail.csx"

using System;

public static Task Run(TimerInfo myTimer, TraceWriter log)
    => SendAdoptionListEmail(log);
