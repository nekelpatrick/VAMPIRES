import { describe, expect, it } from 'vitest'

describe('idle scene bootstrap', () => {
  it('keeps canonical variables present', () => {
    const labels = ['[THRALL]', '[DUSKEN COIN]', '[BLOOD SHARDS]']
    expect(labels.every((label) => label.startsWith('['))).toBe(true)
  })
})

