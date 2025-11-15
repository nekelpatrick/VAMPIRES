import { Types, defineComponent } from 'bitecs'

export const Position = defineComponent({
  x: Types.f32,
  y: Types.f32,
  z: Types.f32
})

export const Velocity = defineComponent({
  x: Types.f32,
  y: Types.f32,
  z: Types.f32
})

export const ThrallTag = defineComponent()

export const Stats = defineComponent({
  attack: Types.f32,
  defense: Types.f32,
  speed: Types.f32,
  critChance: Types.f32
})

export const Health = defineComponent({
  current: Types.f32,
  max: Types.f32
})

export const ActionPoints = defineComponent({
  value: Types.f32,
  threshold: Types.f32
})

export const Faction = defineComponent({
  side: Types.ui8
})

export const Target = defineComponent({
  eid: Types.eid
})

export const SpawnTag = defineComponent({
  wave: Types.ui16,
  elite: Types.ui8
})

export const Reward = defineComponent({
  duskenCoin: Types.f32,
  bloodShards: Types.f32
})

