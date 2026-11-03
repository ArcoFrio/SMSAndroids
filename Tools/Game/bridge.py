"""Commands to the running test copy's two bridges, and their answers.

  python Tools/Game/bridge.py mf "ping" "wait-ingame 120"     # ModForge's bridge (SMSModForge/Tools/Game/bridge.ps1 lists its commands)
  python Tools/Game/bridge.py sa "state" "errors"             # SMSAndroids' (SMSAndroids/TestBridge.cs)

Both read commands.txt in their folder and append "HH:MM:SS.mmm ok|error|timeout ..." lines to results.txt;
every command gets one such answer, so a batch is done when it has as many answers as commands.
"""
import os
import re
import sys
import time

TEST = r"D:\Downloads\SkyrimM\spec\uTorrent\Starmaker Story 1.8E - Androids test"
FOLDERS = {"mf": r"BepInEx\plugins\SMSModForge\DebugBridge", "sa": r"BepInEx\plugins\SMSAndroidsTestBridge\DebugBridge"}
ANSWER = re.compile(r"^\d\d:\d\d:\d\d\.\d{3} (ok|error|timeout)")


class Bridge:
    def __init__(self, which, name="a0", test=TEST):
        self.dir = os.path.join(test, FOLDERS[which], name)
        self.results = os.path.join(self.dir, "results.txt")

    def _lines(self):
        try:
            with open(self.results, encoding="utf-8", errors="replace") as f:
                return f.read().splitlines()
        except FileNotFoundError:
            return []

    def send(self, *commands, timeout=180):
        """Runs the commands in order; returns every line the bridge wrote meanwhile."""
        commands = [c.strip() for c in commands if c and c.strip()]
        before = len(self._lines())
        commands_file = os.path.join(self.dir, "commands.txt")
        # Never over a batch the bridge has not picked up yet.
        deadline = time.time() + timeout
        while os.path.exists(commands_file) and time.time() < deadline:
            time.sleep(0.02)
        tmp = commands_file + ".tmp"
        with open(tmp, "w", encoding="utf-8") as f:
            f.write("\n".join(commands))
        os.replace(tmp, commands_file)
        while time.time() < deadline:
            new = self._lines()[before:]
            if sum(1 for l in new if ANSWER.match(l)) >= len(commands):
                return new
            time.sleep(0.03)
        return self._lines()[before:] + ["(gave up waiting after %d s)" % timeout]

    def ask(self, command, timeout=180):
        """One command; its answer without the time, or '' if none came."""
        answers = [l for l in self.send(command, timeout=timeout) if ANSWER.match(l)]
        return answers[-1][13:] if answers else ""


if __name__ == "__main__":
    for line in Bridge(sys.argv[1]).send(*sys.argv[2:]):
        print(line)
