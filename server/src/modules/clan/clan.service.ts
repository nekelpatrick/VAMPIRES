import type {
  Clan,
  ClanBonus,
  ClanType,
  PlayerClanMembership
} from './clan.schema'
import { CLAN_DEFINITIONS } from './clan.schema'

const playerMemberships: Map<string, PlayerClanMembership> = new Map()
const clanMemberCounts: Map<ClanType, number> = new Map([
  ['CLAN_NOCTURNUM', 0],
  ['CLAN_SABLEHEART', 0],
  ['CLAN_ECLIPSA', 0]
])

export class ClanService {
  getAllClans(): Clan[] {
    return Object.values(CLAN_DEFINITIONS).map((def) => ({
      ...def,
      memberCount: clanMemberCounts.get(def.id) ?? 0
    }))
  }

  getClan(clanId: ClanType): Clan {
    const def = CLAN_DEFINITIONS[clanId]
    return {
      ...def,
      memberCount: clanMemberCounts.get(clanId) ?? 0
    }
  }

  getClanBonuses(clanId: ClanType): ClanBonus {
    return CLAN_DEFINITIONS[clanId].bonuses
  }

  async getPlayerClan(playerId: string): Promise<PlayerClanMembership | null> {
    return playerMemberships.get(playerId) ?? null
  }

  async getPlayerClanBonuses(playerId: string): Promise<ClanBonus | null> {
    const membership = playerMemberships.get(playerId)
    if (!membership) return null
    return this.getClanBonuses(membership.clanId)
  }

  async joinClan(
    playerId: string,
    clanId: ClanType
  ): Promise<PlayerClanMembership> {
    const existing = playerMemberships.get(playerId)
    if (existing) {
      await this.leaveClan(playerId)
    }

    const membership: PlayerClanMembership = {
      playerId,
      clanId,
      joinedAt: new Date(),
      contribution: 0
    }

    playerMemberships.set(playerId, membership)

    const currentCount = clanMemberCounts.get(clanId) ?? 0
    clanMemberCounts.set(clanId, currentCount + 1)

    return membership
  }

  async leaveClan(playerId: string): Promise<boolean> {
    const membership = playerMemberships.get(playerId)
    if (!membership) return false

    const currentCount = clanMemberCounts.get(membership.clanId) ?? 0
    clanMemberCounts.set(membership.clanId, Math.max(0, currentCount - 1))

    playerMemberships.delete(playerId)
    return true
  }

  async addContribution(playerId: string, amount: number): Promise<boolean> {
    const membership = playerMemberships.get(playerId)
    if (!membership) return false

    membership.contribution += amount
    playerMemberships.set(playerId, membership)
    return true
  }

  getClanLeaderboard(clanId: ClanType): PlayerClanMembership[] {
    const members: PlayerClanMembership[] = []
    for (const membership of playerMemberships.values()) {
      if (membership.clanId === clanId) {
        members.push(membership)
      }
    }
    return members.sort((a, b) => b.contribution - a.contribution).slice(0, 100)
  }
}

export const clanService = new ClanService()

