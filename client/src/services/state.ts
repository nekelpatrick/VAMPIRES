import { createStore } from 'zustand/vanilla'

type ResourcePayload = {
  thrallName?: string
  duskenCoin?: number
  bloodShards?: number
}

export type HudState = ResourcePayload & {
  thrallName: string
  duskenCoin: number
  bloodShards: number
  lastSyncedAt: number | null
  setResources: (payload: ResourcePayload) => void
}

export const createHudStore = () =>
  createStore<HudState>((set) => ({
    thrallName: '[WEREWOLF]',
    duskenCoin: 0,
    bloodShards: 0,
    lastSyncedAt: null,
    setResources: (payload) =>
      set((state) => ({
        ...state,
        ...payload,
        lastSyncedAt: Date.now()
      }))
  }))

