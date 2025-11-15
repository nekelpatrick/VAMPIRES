import type { StoreApi } from 'zustand/vanilla'

import type { HudState } from '../services/state'

const abilityNames = ['Rend', 'Howl', 'Pounce', 'Shred', 'Maelstrom']
const navLabels = ['Avatar', 'Habilidade', '[CLAN]', '[ASHEN ONE]', 'Invocar']

export const createHud = (root: HTMLElement, store: StoreApi<HudState>) => {
  root.className = 'hud'

  const top = document.createElement('div')
  top.className = 'hud__top'

  const playerCard = document.createElement('div')
  playerCard.className = 'hud__player'

  const avatar = document.createElement('div')
  avatar.className = 'hud__avatar'
  avatar.textContent = '[THRALL]'

  const playerInfo = document.createElement('div')
  playerInfo.className = 'hud__playerInfo'
  const thrallName = document.createElement('p')
  thrallName.className = 'hud__name'
  const powerScore = document.createElement('span')
  powerScore.className = 'hud__power'
  playerInfo.append(thrallName, powerScore)

  playerCard.append(avatar, playerInfo)

  const currencies = document.createElement('div')
  currencies.className = 'hud__currencies'
  const duskenRow = document.createElement('span')
  duskenRow.className = 'hud__currencyRow'
  const duskenIcon = document.createElement('span')
  duskenIcon.className = 'hud__currencyIcon'
  duskenIcon.style.color = '#f0a202'
  const duskenCoin = document.createElement('span')
  duskenRow.append(duskenIcon, duskenCoin)

  const bloodRow = document.createElement('span')
  bloodRow.className = 'hud__currencyRow'
  const bloodIcon = document.createElement('span')
  bloodIcon.className = 'hud__currencyIcon'
  bloodIcon.style.color = '#d7263d'
  const bloodShards = document.createElement('span')
  bloodRow.append(bloodIcon, bloodShards)

  const cadenceTimer = document.createElement('span')
  cadenceTimer.className = 'hud__timer'

  currencies.append(duskenRow, bloodRow, cadenceTimer)

  top.append(playerCard, currencies)

  const center = document.createElement('div')
  center.className = 'hud__center'

  const energyBar = document.createElement('div')
  energyBar.className = 'hud__energyBar'
  const energyFill = document.createElement('div')
  energyFill.className = 'hud__energyFill'
  energyBar.append(energyFill)

  const statusBar = document.createElement('div')
  statusBar.className = 'hud__status'
  const waveLabel = document.createElement('span')
  const killLabel = document.createElement('span')
  const lifeLabel = document.createElement('span')
  statusBar.append(waveLabel, killLabel, lifeLabel)

  const abilityRow = document.createElement('div')
  abilityRow.className = 'hud__abilities'
  const abilitySlots = abilityNames.map((name) => {
    const slot = document.createElement('div')
    slot.className = 'hud__ability'
    slot.textContent = name
    abilityRow.append(slot)
    return slot
  })

  center.append(energyBar, statusBar, abilityRow)

  const bottom = document.createElement('div')
  bottom.className = 'hud__bottom'

  const inventory = document.createElement('div')
  inventory.className = 'hud__inventory'
  const slotElements = Array.from({ length: 8 }).map((_, index) => {
    const slot = document.createElement('div')
    slot.className = 'hud__slot'
    slot.textContent = `LV.${index + 60}`
    inventory.append(slot)
    return slot
  })

  const navBar = document.createElement('div')
  navBar.className = 'hud__nav'
  navLabels.forEach((label) => {
    const button = document.createElement('button')
    button.type = 'button'
    button.textContent = label
    navBar.append(button)
  })

  const pendingSync = document.createElement('span')
  pendingSync.className = 'hud__pending'

  bottom.append(inventory, navBar, pendingSync)

  root.append(top, center, bottom)

  const render = (state: HudState) => {
    thrallName.textContent = `${state.thrallName} [THRALL]`
    const attackRating = Math.round((state.wave + 1) * 1.6 + state.duskenCoin / 1000)
    powerScore.textContent = `${attackRating.toLocaleString()} ATK`
    waveLabel.textContent = `Wave ${state.wave}`
    killLabel.textContent = `${state.kills} Slain`
    lifeLabel.textContent = state.thrallAlive ? 'Engaged' : '[DEATH]'
    duskenCoin.textContent = `${state.duskenCoin.toLocaleString()} [DUSKEN COIN]`
    bloodShards.textContent = `${state.bloodShards.toLocaleString()} [BLOOD SHARDS]`
    const pendingDusken = state.pendingDuskenCoin
    const pendingBlood = state.pendingBloodShards
    const cadenceSeconds = Math.max(3, 30 - state.wave)
    cadenceTimer.textContent = `${cadenceSeconds}s cadence`
    if (pendingDusken > 0 || pendingBlood > 0) {
      pendingSync.textContent = `Syncing +${pendingDusken} / +${pendingBlood}`
    } else {
      pendingSync.textContent = 'Balances synced'
    }
    const energyPercent = Math.min(100, (state.kills % 40) * 2.5 + (state.pendingDuskenCoin % 20))
    energyFill.style.width = `${energyPercent}%`
    slotElements.forEach((slot, index) => {
      const level = Math.max(1, state.wave + index)
      slot.textContent = `LV.${level}`
    })
    abilitySlots.forEach((slot, index) => {
      const active = state.wave % abilitySlots.length === index
      slot.classList.toggle('hud__ability--active', active)
    })
  }

  render(store.getState())
  const unsubscribe = store.subscribe(render)
  return unsubscribe
}

