import type { Player } from './player.schema'

const SAMPLE_PLAYER: Player = {
  id: 'player-ash',
  displayName: '[PLAYER]',
  clan: 'CLAN NOCTURNUM',
  duskenCoinBalance: 1000,
  bloodShardBalance: 5,
  premiumStatus: '[ASHEN ONE]'
}

export class PlayerService {
  getProfile(playerId: string): Promise<Player> {
    return Promise.resolve({
      ...SAMPLE_PLAYER,
      id: playerId
    })
  }
}

export const playerService = new PlayerService()

