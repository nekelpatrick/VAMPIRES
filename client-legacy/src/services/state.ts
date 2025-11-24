import { createStore } from 'zustand/vanilla'

type ResourcePayload = {
  thrallName?: string
  duskenCoin?: number
  bloodShards?: number
  thrallHp?: number
  thrallHpMax?: number
}

type CurrencyDelta = {
  duskenCoin?: number
  bloodShards?: number
}

type CombatPayload = {
  wave?: number
  kills?: number
  thrallAlive?: boolean
  pendingDuskenCoin?: number
  pendingBloodShards?: number
}

export type HudState = {
  thrallName: string
  duskenCoin: number
  bloodShards: number
  thrallHp: number
  thrallHpMax: number
  wave: number
  kills: number
  thrallAlive: boolean
  pendingDuskenCoin: number
  pendingBloodShards: number
  lastSyncedAt: number | null
  setResources: (payload: ResourcePayload) => void
  setCombat: (payload: CombatPayload) => void
  applyCurrencyDelta: (delta: CurrencyDelta) => void
  applyCombat: (payload: CombatPayload) => void
  markSynced: (balances?: CurrencyDelta) => void
}

export const createHudStore = () =>
  createStore<HudState>((set) => ({
    thrallName: '[WEREWOLF]',
    duskenCoin: 0,
    bloodShards: 0,
    thrallHp: 0,
    thrallHpMax: 0,
    wave: 1,
    kills: 0,
    thrallAlive: true,
    pendingDuskenCoin: 0,
    pendingBloodShards: 0,
    lastSyncedAt: null,
    setResources: (payload) =>
      set((state) => ({
        ...state,
        ...payload,
        pendingDuskenCoin: 0,
        pendingBloodShards: 0,
        lastSyncedAt: Date.now()
      })),
    setCombat: (payload) =>
      set((state) => ({
        ...state,
        wave: payload.wave ?? state.wave,
        kills: payload.kills ?? state.kills,
        thrallAlive: payload.thrallAlive ?? state.thrallAlive
      })),
    applyCurrencyDelta: (delta) =>
      set((state) => ({
        ...state,
        duskenCoin: state.duskenCoin + (delta.duskenCoin ?? 0),
        bloodShards: state.bloodShards + (delta.bloodShards ?? 0)
      })),
    applyCombat: (payload) =>
      set((state) => ({
        ...state,
        wave: payload.wave ?? state.wave,
        kills: payload.kills ?? state.kills,
        thrallAlive: payload.thrallAlive ?? state.thrallAlive,
        pendingDuskenCoin: payload.pendingDuskenCoin ?? state.pendingDuskenCoin,
        pendingBloodShards: payload.pendingBloodShards ?? state.pendingBloodShards
      })),
    markSynced: (balances) =>
      set((state) => ({
        ...state,
        duskenCoin: balances?.duskenCoin ?? state.duskenCoin,
        bloodShards: balances?.bloodShards ?? state.bloodShards,
        pendingDuskenCoin: 0,
        pendingBloodShards: 0,
        lastSyncedAt: Date.now()
      }))
  }))


