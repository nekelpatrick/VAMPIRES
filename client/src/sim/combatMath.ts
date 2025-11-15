export const computeDamage = (attack: number, defense: number) => {
  const mitigated = attack - defense * 0.5
  return Math.max(1, Math.floor(mitigated))
}

export const applyVariance = (baseDamage: number, variance: number, roll: number) => {
  const spread = baseDamage * variance
  return Math.max(1, Math.floor(baseDamage + (roll - 0.5) * spread))
}


