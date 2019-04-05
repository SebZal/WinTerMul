## WinTerMul - Windows Terminal Multiplexer

A terminal multiplexer for Windows.

#### TODO
- [x] Error handling, write error logs to file system.
- [ ] Write tests.
- [x] Add license.
- [ ] Document key shortcuts.
- [x] Remove PInvoke dependency.
- [x] Go through all kernel32 calls and handle errors.
- [x] Handle terminal resize.
- [x] Make sure all child processes are killed before parent process closes.
- [x] Kill parent process if all child processes are killed.
- [x] Display caret (only in active pane), handle insert mode as well. Don't display caret in vifm.
- [x] Move caret to active pane after pane switch.
- [x] Reduce CPU usage.
- [x] Add configuration file.
- [x] Cleanup all TODOs in source code.
- [x] Add info logs.
- [ ] Large buffers causes empty console, fix this.
- [ ] When terminal is opened from existing console the buffer size is too small.
- [ ] Setup build.
- [ ] Setup GitHub project with further work.
- [ ] Create first release and split branches into master and develop (setup branch policies).
