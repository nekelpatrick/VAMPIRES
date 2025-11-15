import type { StoreApi } from 'zustand/vanilla'

import type { HudState } from '../services/state'

export const createHud = (root: HTMLElement, store: StoreApi<HudState>) => {
  root.className = 'hud'

  const thrallName = document.createElement('p')
  thrallName.className = 'hud__thrall'

  const currencyBar = document.createElement('div')
  currencyBar.className = 'hud__currencies'

  const duskenCoin = document.createElement('span')
  duskenCoin.className = 'hud__currency hud__currency--dusken'

  const bloodShards = document.createElement('span')
  bloodShards.className = 'hud__currency hud__currency--blood'

  currencyBar.append(duskenCoin, bloodShards)
  root.append(thrallName, currencyBar)

  const render = (state: HudState) => {
    thrallName.textContent = `${state.thrallName} [THRALL]`
    duskenCoin.textContent = `${state.duskenCoin.toLocaleString()} [DUSKEN COIN]`
    bloodShards.textContent = `${state.bloodShards.toLocaleString()} [BLOOD SHARDS]`
  }

  render(store.getState())
  const unsubscribe = store.subscribe(render)
  return unsubscribe
}

