# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity project containing **PolicyDrivenSingleton**, a policy-driven singleton base class for MonoBehaviour that works with both Domain Reload ON and OFF settings. The main library code is located in `Assets/Plugins/unity-policy-driven-singleton/PolicyDrivenSingleton/`.

The project includes:
- **Runtime library**: Core singleton implementation with policy-based behavior
- **Editor support**: Play Mode state management hooks
- **Comprehensive test suite**: 79 tests (58 PlayMode + 21 EditMode)

## Common Commands

### Running Tests

Tests are run through Unity's Test Runner window:
```
Window → General → Test Runner → Run All
```

For command-line test execution (if Unity Test Framework CLI is configured):
```bash
# Run all tests
Unity.exe -runTests -testPlatform PlayMode -projectPath . -testResults results.xml

# Run EditMode tests only
Unity.exe -runTests -testPlatform EditMode -projectPath . -testResults results.xml
```

### Building

This is a library project - there is no traditional "build" step. The library is consumed by copying the `PolicyDrivenSingleton/` folder into Unity projects.

### Code Quality

The project uses Unity's built-in code analysis. No external linters or formatters are configured.

## High-Level Architecture

### Policy-Driven Design

The singleton implementation uses **compile-time policy resolution** via generic type parameters:

```
SingletonBehaviour<T, TPolicy>
    ├─ GlobalSingleton<T> (uses PersistentPolicy)
    └─ SceneSingleton<T> (uses SceneScopedPolicy)
```

**Key insight**: Policies are `readonly struct` implementing `ISingletonPolicy`. Using `default(TPolicy)` enables zero-allocation policy resolution at compile-time.

### Domain Reload OFF Support

The library uses `PlaySessionId` to invalidate static caches across Play sessions:

1. **SubsystemRegistration** callback increments `PlaySessionId` on each Play Mode entry
2. Each singleton tracks its `_initializedPlaySessionId`
3. On `Instance` access, if `PlaySessionId` changed → invalidate cache → re-search → re-establish
4. This allows static fields to persist while ensuring proper re-initialization

**Critical files**:
- `SingletonRuntime.cs`: Manages `PlaySessionId`, `IsQuitting` state, thread validation
- `SingletonBehaviour.cs`: `InvalidateInstanceCacheIfPlaySessionChanged()` method

### Type Safety & Cache Management

**Strict type matching** (`AsExactType` / `TryEstablishAsInstance`):
- Only exact `T` instances are accepted
- Derived types are rejected with error log + destruction
- Prevents accidental type mismatches when using inheritance

**Static cache per closed generic type**:
- `static T _instance` in `SingletonBehaviour<T, TPolicy>` creates a separate cache for each `(T, TPolicy)` pair
- Example: `GameManager : GlobalSingleton<GameManager>` gets its own `_instance` field
- ReSharper warnings suppressed because this is intentional per-type state

### Edit Mode vs Play Mode

**Edit Mode behavior** (`Instance` / `TryGetInstance` EditMode paths):
- Search only - no auto-creation
- No static cache updates
- Prevents side effects in Editor extensions

**Play Mode behavior**:
- Full singleton lifecycle
- Static cache management
- Auto-creation for `GlobalSingleton<T>`
- Fail-fast in DEV/EDITOR/ASSERTIONS, fail-soft in Release builds

### Thread Safety

All singleton operations MUST be called from the main Unity thread:
- `SingletonRuntime.ValidateMainThread()` checks thread ID
- Background threads receive `null` + error log
- Use `TryPostToMainThread(Action)` to dispatch from background threads via `SynchronizationContext`

### Lifecycle Hooks

**Standard Unity lifecycle**:
```
Awake() → OnEnable() → [Play] → OnDisable() → OnDestroy()
```

**Singleton-specific hook**:
- `OnPlaySessionStart()`: Called once per Play session via `InitializeForCurrentPlaySessionIfNeeded`
  - First Play (Domain Reload ON): `Awake()` → `OnPlaySessionStart()`
  - Subsequent Play (Domain Reload OFF): `OnPlaySessionStart()` only
  - Use for idempotent re-initialization (event subscriptions, temporary data reset)

**Requirement**: Subclasses MUST call `base.Awake()` if overriding. Detected at runtime via `_baseAwakeCalled` in `OnEnable`.

## Critical Constraints

### For Library Users

1. **Sealed classes recommended**: Prevents type mismatch issues with inheritance
2. **Active & Enabled**: Singletons must be active. Inactive/disabled instances are invalid and fail-fast when detected in DEV builds
3. **Main thread only**: All `Instance`/`TryGetInstance` calls from main thread
4. **SceneSingleton requires scene placement**: No auto-creation - must be pre-placed
5. **Termination handling**: Use `TryGetInstance` in `OnDestroy`/`OnDisable` to avoid resurrection during app quit

### For Library Development

1. **InternalsVisibleTo**: `AssemblyInfo.cs` exposes internals to test assemblies
2. **Conditional logging**: All logging uses `[Conditional("UNITY_EDITOR")]` or similar - stripped in Release builds
3. **Fail-fast vs Fail-soft**:
   - DEV/EDITOR/ASSERTIONS: Throw exceptions on violations
   - Player builds: Return `null`/`false` silently
4. **FindObjectsInactive.Exclude**: Inactive objects are not searched by default - intentional to prevent hidden duplicates

## Assembly Definitions

```
PolicyDrivenSingleton (Runtime)
  └─ PolicyDrivenSingleton.Editor (Editor-only)
       └─ PolicyDrivenSingleton.Editor.Tests (Editor tests)
  └─ PolicyDrivenSingleton.Tests (Runtime/PlayMode tests)
```

**No external dependencies** - the library is self-contained.

## Directory Structure (Library Code)

```
Assets/Plugins/unity-policy-driven-singleton/PolicyDrivenSingleton/
├── Core/
│   ├── SingletonBehaviour.cs          # Main implementation (~370 lines)
│   ├── SingletonRuntime.cs            # PlaySessionId, IsQuitting, threading (~200 lines)
│   ├── SingletonLogger.cs             # Conditional logging (~120 lines)
│   └── AssemblyInfo.cs                # InternalsVisibleTo for tests
├── Policy/
│   ├── ISingletonPolicy.cs            # Policy interface
│   ├── PersistentPolicy.cs            # GlobalSingleton policy
│   └── SceneScopedPolicy.cs           # SceneSingleton policy
├── Editor/
│   └── SingletonEditorHooks.cs        # Play Mode state management
├── Tests/
│   ├── Runtime/                       # PlayMode tests (58 tests)
│   ├── Editor/                        # EditMode tests (21 tests)
│   └── TestExtensions.cs              # Test helpers
├── GlobalSingleton.cs                 # Public API (~30 lines)
├── SceneSingleton.cs                  # Public API (~35 lines)
└── PolicyDrivenSingleton.asmdef       # Runtime assembly definition
```

## Development Notes

### Testing Philosophy

Tests cover:
- **Lifecycle**: Creation, caching, destruction, re-establishment
- **Domain Reload OFF**: PlaySessionId invalidation, soft reset
- **Thread safety**: Background thread rejection, SynchronizationContext posting
- **Policy behavior**: Auto-creation vs manual placement
- **Edge cases**: Type mismatch, inactive instances, duplicate detection, termination guards

**Test isolation**: Each test uses unique singleton types to prevent cross-test contamination from static caches.

### Key Implementation Details

**Duplicate detection** (`InitializeForCurrentPlaySessionIfNeeded` / `TryEstablishAsInstance`):
- DEV/EDITOR/ASSERTIONS: before cache establishment, scan for multiple exact instances and fail-fast
- Normal path: if cache exists and a duplicate appears, it is destroyed with a warning

**DontDestroyOnLoad enforcement** (`EnsurePersistent`):
- Must be called on root GameObject
- If component is on child object → auto-reparent to root + warning
- Prevents silent failure of persistence

**Best-effort termination guard** (`SingletonRuntime.NotifyQuitting` + editor hooks):
- `Application.quitting` callback sets `IsQuitting = true`
- Not 100% reliable (crashes, force-quit, cancellation edge cases)
- Editor hooks reset flag on Play Mode state change

### Logging Levels

All logs are conditional - stripped in Release builds:

| Level | When | Example Triggers |
|-------|------|-----------------|
| **Log** | Normal operation | OnPlaySessionStart invoked, quit access |
| **Warning** | Recoverable issues | Auto-creation, duplicates, reparenting |
| **Error** | Violations (DEV only) | Type mismatch, inactive instance, base.Awake() missing, thread violations |

## README Integration

The README (`Assets/Plugins/unity-policy-driven-singleton/README.md`) is comprehensive (600+ lines) with:
- Mermaid diagrams for architecture and flow
- API reference tables
- Usage examples
- Troubleshooting guide

When making changes:
1. Update code documentation in source files
2. If API surface changes, update Quick Start and API sections in README
3. If architecture changes, update Mermaid diagrams
4. Add new limitations/constraints to Known Limitations section
