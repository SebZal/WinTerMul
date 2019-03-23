## WinTerMul - Windows Terminal Multiplexer

A terminal multiplexer for Windows.

#### TODO
- [ ] Error handling, write error logs to file system.
- [ ] Write tests.
- [x] Add license.
- [ ] Document key shortcuts.
- [ ] Remove PInvoke dependency.
- [ ] Go through all kernel32 calls and handle errors.
- [x] Handle terminal resize.
- [x] Make sure all child processes are killed before parent process closes.
- [x] Kill parent process if all child processes are killed.
- [x] Display caret (only in active pane), handle insert mode as well. Don't display caret in vifm.
- [ ] Move caret to active pane after pane switch.
- [ ] Cleanup all TODOs in source code.
