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

