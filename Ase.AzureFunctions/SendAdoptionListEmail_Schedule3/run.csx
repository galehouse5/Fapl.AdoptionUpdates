#r "..\Shared\Ase.Shared.dll"
#r "System.Configuration"

#load "..\SendAdoptionListEmail.csx"

using System;

public static Task Run(TimerInfo myTimer, TraceWriter log)
    => SendAdoptionListEmail(log);
