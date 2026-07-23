# 🐛 BugBuddy

**Your friendly .NET build error explainer** — Stop staring at cryptic compiler errors! BugBuddy explains them in plain English and tells you exactly how to fix them.

[![.NET](https://img.shields.io/badge/.NET-8.0+-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen?style=flat-square)](http://makeapullrequest.com)

---

## ✨ What is BugBuddy?

BugBuddy is a .NET Global Tool that wraps `dotnet build` and transforms scary compiler errors into friendly, actionable explanations. Think of it as a supportive coding buddy who never judges you for missing a semicolon.

### Before BugBuddy 😰
```
error CS1002: ; expected
error CS0246: The type or namespace name 'ILogger' could not be found
```

### After BugBuddy 😊
```
╭── 🔴 CS1002 — Program.cs (Line 15) ──╮
│                                        │
│  💬 What happened:                     │
│     Hey! Looks like you forgot a       │
│     semicolon (;) at the end of a      │
│     statement.                         │
│                                        │
│  🔧 How to fix:                        │
│     Add a ';' at the end of the line   │
│     — easy fix!                        │
│                                        │
│  💡 Example:                           │
│     int x = 5;  // Don't forget → ;    │
│                                        │
╰────────────────────────────────────────╯
```

---

## 🚀 Installation

```bash
# Install as a global tool
dotnet tool install -g BugBuddy

# Or install from source
git clone https://github.com/yourusername/BugBuddy.git
cd BugBuddy
dotnet pack
dotnet tool install -g --add-source ./nupkg BugBuddy
```

## 📖 Usage

### Build & Explain

```bash
# Build current directory and explain errors
bugbuddy build

# Build a specific project
bugbuddy build ./src/MyProject

# Build a solution file
bugbuddy build ./MySolution.sln
```

### Analyze a Specific Error Code

```bash
# Look up any C# error code
bugbuddy analyze CS1002
bugbuddy analyze CS0246
bugbuddy analyze CS8602
```

### Configure AI (Optional)

BugBuddy works great out of the box with 30+ built-in error explanations. For even smarter, context-aware explanations, add your OpenAI API key:

```bash
# Set your OpenAI API key
bugbuddy config --api-key sk-your-key-here

# Change the AI model (default: gpt-4o-mini)
bugbuddy config --model gpt-4o

# View current settings
bugbuddy config --show
```

---

## 🧠 How It Works

```
dotnet build → Capture Output → Parse Errors → Explain → Pretty Print
                                     │              │
                                     ▼              ▼
                              MSBuild Regex    AI (OpenAI API)
                                               or Built-in Dictionary
```

1. **Build Runner** — Executes `dotnet build` and captures stdout/stderr
2. **Error Parser** — Uses regex to parse MSBuild's standard error format
3. **Explainer** — AI-powered (OpenAI) or built-in dictionary with 30+ error codes
4. **Renderer** — Beautiful terminal output with [Spectre.Console](https://spectreconsole.net/)

---

## 🎯 Supported Error Codes (Built-in)

BugBuddy has friendly explanations for 30+ common C# errors including:

| Code | Description |
|------|-------------|
| CS1002 | Missing semicolon |
| CS0246 | Type or namespace not found |
| CS0103 | Name does not exist in scope |
| CS1061 | Type doesn't contain member |
| CS0029 | Cannot implicitly convert type |
| CS8600-CS8625 | Nullable reference warnings |
| ...and many more! | |

> 💡 With an OpenAI API key, BugBuddy can explain **any** error code with context-aware explanations.

---

## 🛠️ Tech Stack

- **.NET 8.0** — Cross-platform runtime
- **System.CommandLine** — CLI argument parsing
- **Spectre.Console** — Beautiful terminal UI
- **OpenAI API** — AI-powered explanations (optional)

---

## 🤝 Contributing

Contributions are welcome! Here are some ways you can help:

- 🐛 **Add more error codes** to the built-in dictionary
- 🌍 **Add language support** (Turkish, Spanish, etc.)
- 🧪 **Write tests** for the error parser
- 📝 **Improve documentation**

```bash
# Clone & run locally
git clone https://github.com/yourusername/BugBuddy.git
cd BugBuddy
dotnet run -- build ./test-project
```

---

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

<p align="center">
  Made with ❤️ by developers, for developers<br>
  <b>Because error messages should be helpful, not scary.</b>
</p>
