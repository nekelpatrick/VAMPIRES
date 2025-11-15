import type { Thrall } from './thrall.schema'

const SAMPLE_THRALL: Thrall = {
  id: '[THRALL]-alpha',
  archetype: '[WEREWOLF]',
  level: 1,
  hp: 150,
  attack: 25,
  defense: 10,
  status: 'ACTIVE'
}

export class ThrallService {
  getActiveThrall(playerId: string): Promise<Thrall> {
    return Promise.resolve({
      ...SAMPLE_THRALL,
      id: `${playerId}-thrall`
    })
  }
}

export const thrallService = new ThrallService()

