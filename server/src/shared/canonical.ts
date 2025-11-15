const CANONICAL_VARIABLES = [
  '[MOBILE]',
  '[GAIN MONEY]',
  '[THRALL]',
  '[PLAYER]',
  '[VAMPIRE]',
  '[WEREWOLF]',
  '[HORDE]',
  '[CLAN]',
  '[BATTLEFIELD]',
  '[NPC]',
  '[DEATH]',
  '[STRONGER]',
  '[TRHALL]',
  '[DUSKEN COIN]',
  '[BLOOD SHARDS]',
  '[PREMIUM]',
  '[ASHEN ONE]'
] as const

type CanonicalVariable = (typeof CANONICAL_VARIABLES)[number]

export { CANONICAL_VARIABLES }
export type { CanonicalVariable }

