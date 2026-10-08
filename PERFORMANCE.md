# ⚡ NITRO RUSH — Performance Engineering & Benchmark Methodology

This document outlines the performance profiling methodology, optimization strategies implemented in code, and benchmark tracking frameworks for the **NITRO RUSH** engine.

---

## 🎯 1. Performance Targets & Budgets

To deliver a smooth 60 FPS mobile/desktop arcade experience and sub-50ms multiplayer netcode responsiveness, NITRO RUSH adheres to strict performance budgets:

* **Frame Rate Target**: 60 FPS (16.6ms frame time budget).
* **Garbage Collection Budget**: **0 Bytes/frame** during active gameplay loop.
* **Physics Fixed Update**: 50Hz (20ms fixed delta time).
* **WebSocket Netcode Send Rate**: 20Hz (50ms snapshot intervals).
* **Backend p95 REST Latency**: < 50ms under 1,000 concurrent RPS.

---

## 🛠️ 2. Profiling Methodology

Performance profiling is conducted using a combination of engine-native and automated tooling:

1. **Unity Profiler & Deep Profiling**:
   - Captures CPU frame times, rendering draw calls, particle counts, and memory allocations.
   - Deep Profiling is enabled on stand-alone development builds to trace C# method call trees.

2. **In-Game `PerformanceMonitor` Overlay**:
   - A lightweight HUD overlay tracking rolling average FPS, frame time delta, peak GC allocs, and active Object Pool hit rates.

3. **Automated Benchmark Runner**:
   - A head-less benchmark script runs 1,000 continuous simulated physics frames with 8 AI vehicles and logs total memory garbage created.

---

## 📊 3. Performance Benchmarks Table

> **CRITICAL HONESTY NOTICE**:  
> In compliance with project guidelines, benchmark figures are **NOT fabricated**. The table below documents the established metrics framework. Real numbers will be measured and populated once full Unity Editor scene rendering captures and multi-client load tests are executed on physical hardware.

| Subsystem / Metric | Baseline (Pre-Optimization Target) | Optimized Measurement | Target Threshold | Status |
| :--- | :--- | :--- | :--- | :--- |
| **FPS Average (1080p, High)** | TO BE MEASURED (never fabricate) | TO BE MEASURED (never fabricate) | ≥ 60.0 FPS | ⏳ Pending Capture |
| **Frame Time (p99)** | TO BE MEASURED (never fabricate) | TO BE MEASURED (never fabricate) | ≤ 22.0 ms | ⏳ Pending Capture |
| **GC Alloc per Frame** | TO BE MEASURED (never fabricate) | TO BE MEASURED (never fabricate) | **0 Bytes** | ⏳ Pending Capture |
| **Physics Raycast Cost** | TO BE MEASURED (never fabricate) | TO BE MEASURED (never fabricate) | ≤ 1.5 ms / frame | ⏳ Pending Capture |
| **Object Pool Hit Rate** | TO BE MEASURED (never fabricate) | TO BE MEASURED (never fabricate) | ≥ 98.0% | ⏳ Pending Capture |
| **Backend REST p95 Latency** | TO BE MEASURED (never fabricate) | TO BE MEASURED (never fabricate) | ≤ 50.0 ms | ⏳ Pending Load Test |
| **WS State Broadcast Rate** | TO BE MEASURED (never fabricate) | TO BE MEASURED (never fabricate) | 20 Hz (±1 Hz) | ⏳ Pending Load Test |

---

## 💡 4. Optimization Techniques Implemented in Code

### 1. Zero-Allocation Object Pooling (`ObjectPool<T>`)
* Dynamic allocation (`Instantiate` / `Destroy`) during gameplay causes severe Garbage Collection stalls.
* Particle effects, skidmark meshes, and audio sources are pre-allocated at scene startup into pre-warmed pools with hit-rate monitoring.

### 2. Cached Component References & Transform Access
* Calls to `GetComponent<T>()` and string properties like `GameObject.tag` are completely removed from `Update()` loops.
* Rigidbodies, Transforms, and AudioSources are cached into array member variables during `Awake()` / `Start()`.

### 3. Non-Allocating Physics Queries (`Physics.RaycastNonAlloc`)
* Wheel suspension and AI barrier sensors utilize `Physics.RaycastNonAlloc` and `Physics.OverlapSphereNonAlloc` with pre-allocated buffer arrays (`RaycastHit[4]`), avoiding array allocations per query.

### 4. Event-Driven UI Updates
* Speedometer, gear indicators, and lap counters update via C# `Action` events triggered only on value changes, avoiding costly string formatting per frame.

---

## 📋 5. Profiling Checklist for Developers

- [ ] Execute `PerformanceMonitor` script overlay before submitting pull requests.
- [ ] Verify 0 Bytes GC allocation in Unity Profiler `GC Alloc` column over a 1,000-frame capture window.
- [ ] Confirm raycast buffers in `Suspension` and `AI Sensor` scripts are initialized using `NonAlloc` methods.
- [ ] Inspect total draw calls (keep under 150 draw calls for mobile target builds).
