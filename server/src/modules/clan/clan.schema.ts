import { z } from 'zod'

export const clanTypeSchema = z.enum([
  'CLAN_NOCTURNUM',
  'CLAN_SABLEHEART',
  'CLAN_ECLIPSA'
])

export type ClanType = z.infer<typeof clanTypeSchema>

export const clanBonusSchema = z.object({
  lifestealBonus: z.number().default(0),
  attackSpeedBonus: z.number().default(0),
  bleedChanceBonus: z.number().default(0),
  curseChanceBonus: z.number().default(0)
})

export type ClanBonus = z.infer<typeof clanBonusSchema>

export const clanSchema = z.object({
  id: clanTypeSchema,
  name: z.string(),
  description: z.string(),
  bonuses: clanBonusSchema,
  memberCount: z.number().int().nonnegative().default(0)
})

export type Clan = z.infer<typeof clanSchema>

export const playerClanMembershipSchema = z.object({
  playerId: z.string().min(1),
  clanId: clanTypeSchema,
  joinedAt: z.date(),
  contribution: z.number().int().nonnegative().default(0)
})

export type PlayerClanMembership = z.infer<typeof playerClanMembershipSchema>

export const CLAN_DEFINITIONS: Record<ClanType, Omit<Clan, 'memberCount'>> = {
  CLAN_NOCTURNUM: {
    id: 'CLAN_NOCTURNUM',
    name: 'Clan Nocturnum',
    description:
      'Ancient royalty. Masters of blood rituals. Passive lifesteal bonuses.',
    bonuses: {
      lifestealBonus: 0.05,
      attackSpeedBonus: 0,
      bleedChanceBonus: 0,
      curseChanceBonus: 0
    }
  },
  CLAN_SABLEHEART: {
    id: 'CLAN_SABLEHEART',
    name: 'Clan Sableheart',
    description:
      'Martial aristocrats. Known for brutality and blade mastery. Attack/crit bonuses.',
    bonuses: {
      lifestealBonus: 0,
      attackSpeedBonus: 0.1,
      bleedChanceBonus: 0,
      curseChanceBonus: 0
    }
  },
  CLAN_ECLIPSA: {
    id: 'CLAN_ECLIPSA',
    name: 'Clan Eclipsa',
    description:
      'Arcane manipulators and shadowbinders. Status effects, curses, and bleed bonuses.',
    bonuses: {
      lifestealBonus: 0,
      attackSpeedBonus: 0,
      bleedChanceBonus: 0.15,
      curseChanceBonus: 0.1
    }
  }
}

export const joinClanRequestSchema = z.object({
  playerId: z.string().min(1),
  clanId: clanTypeSchema
})

export type JoinClanRequest = z.infer<typeof joinClanRequestSchema>

export const leaveClanRequestSchema = z.object({
  playerId: z.string().min(1)
})

export type LeaveClanRequest = z.infer<typeof leaveClanRequestSchema>

export const getClanBonusesRequestSchema = z.object({
  clanId: clanTypeSchema
})

export type GetClanBonusesRequest = z.infer<typeof getClanBonusesRequestSchema>

