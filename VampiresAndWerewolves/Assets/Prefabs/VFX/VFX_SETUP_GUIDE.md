# VFX Graph Setup Guide

Create the following VFX Graph assets in Unity Editor (Window > Visual Effects > Visual Effect Graph):

## Blood Effects

### BloodSplash.vfx
- **Spawn**: Burst of 10-30 particles on event
- **Shape**: Cone spray in Direction parameter
- **Color**: Dark red (#8B0000) to bright red (#FF0000)
- **Size**: 0.05 to 0.15 over lifetime (shrink)
- **Lifetime**: 0.5 to 1.5 seconds
- **Gravity**: Strong downward force (-15)
- **Exposed**: `ParticleCount` (int), `Direction` (Vector3)

### BloodDrip.vfx
- **Spawn**: Constant rate (5/sec) while active
- **Shape**: Downward only
- **Color**: Dark red with alpha fade
- **Size**: Small droplets (0.02-0.05)
- **Lifetime**: 2 seconds
- **Gravity**: Medium (-10)

### DeathExplosion.vfx
- **Spawn**: Large burst (50-100 particles)
- **Shape**: Sphere explosion outward
- **Color**: Red to dark brown
- **Size**: Varied (0.05-0.3)
- **Lifetime**: 1-3 seconds
- **Sub-effects**: Organ chunks (larger particles), blood mist

### GroundDecal.vfx
- **Type**: Output Mesh (Quad)
- **Color**: Dark red (#4A0000) with alpha 0.7
- **Size**: 0.3 to 1.0 random
- **Lifetime**: 10 seconds with fade out
- **Orientation**: Flat on ground (Y-up normal)

## Ability Effects

### LifestealEffect.vfx
- **Type**: Trail from source to target
- **Color**: Green (#00FF00) to white
- **Exposed**: `TargetPosition` (Vector3)
- **Motion**: Curved arc toward target

### BleedEffect.vfx
- **Type**: Attached drip particles
- **Color**: Blood red
- **Spawn**: Periodic pulses
- **Size**: Small droplets

### StunEffect.vfx
- **Type**: Star burst + circling stars
- **Color**: Yellow/white
- **Shape**: Rings around head position

### RageAura.vfx
- **Type**: Ground aura + rising particles
- **Color**: Red/orange flames
- **Shape**: Cylinder around character
- **Motion**: Upward spiral

### HowlWave.vfx
- **Type**: Expanding ring
- **Color**: Blue/white energy
- **Exposed**: `Radius` (float)
- **Motion**: Rapid expansion then fade

## Setup Steps

1. Create each VFX Graph asset in `Assets/Prefabs/VFX/`
2. Create VFXConfig asset: Right-click > Create > Vampires > VFXConfig
3. Assign all VFX assets to the config
4. Add VFXManager to scene and assign the config

