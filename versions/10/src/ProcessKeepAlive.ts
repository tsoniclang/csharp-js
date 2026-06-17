import {
  ManualResetEventSlim,
  Monitor,
  Thread,
} from "@tsonic/dotnet/System.Threading.js";

class SyncRoot {}

const sync = new SyncRoot();

let refCount = 0;
let releaseSignal: ManualResetEventSlim | null = null;
let keeperThread: Thread | null = null;

export const Acquire = (): void => {
  Monitor.Enter(sync);
  try {
    refCount += 1;
    if (refCount !== 1) {
      return;
    }

    const signal = new ManualResetEventSlim(false);
    const thread = new Thread(() => {
      signal.Wait();
    });
    thread.IsBackground = false;
    thread.Name = "Tsonic.ProcessKeepAlive";

    releaseSignal = signal;
    keeperThread = thread;
    thread.Start();
  } finally {
    Monitor.Exit(sync);
  }
};

export const Release = (): void => {
  let signal: ManualResetEventSlim | null = null;

  Monitor.Enter(sync);
  try {
    if (refCount === 0) {
      return;
    }

    refCount -= 1;
    if (refCount !== 0) {
      return;
    }

    signal = releaseSignal;
    releaseSignal = null;
    keeperThread = null;
  } finally {
    Monitor.Exit(sync);
  }

  signal?.Set();
};
