## WinTerMul - Windows Terminal Multiplexer

[![Build Status](https://dev.azure.com/sebaza/WinTerMul/_apis/build/status/SebZal.WinTerMul?branchName=master)](https://dev.azure.com/sebaza/WinTerMul/_build/latest?definitionId=1&branchName=master)

A simple terminal multiplexer for Windows.

#### Keyboard shortcuts

Keyboard shortcuts are modifiable through the WinTerMul.Common/appsettings.json file.
The following are the default shortcuts:

```
PrefixKey: CTRL+k
SetNextTerminalActiveKey: l
SetPreviousTerminalActive: h
VerticalSplitKey: v
ClosePaneKey: x
```

#### Known issues
- Large buffers causes an empty console.
- When terminal is opened from existing console the buffer size is too small.
