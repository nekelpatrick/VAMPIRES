import { createWorld } from 'bitecs'
import type { IWorld } from 'bitecs'

export type GameWorld = IWorld

export const createGameWorld = () => createWorld()

