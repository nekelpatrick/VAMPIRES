export class SeededRng {
  private state: number

  constructor(seed: number) {
    this.state = seed >>> 0
  }

  next() {
    let x = this.state
    x += 0x6d2b79f5
    x = Math.imul(x ^ (x >>> 15), x | 1)
    x ^= x + Math.imul(x ^ (x >>> 7), x | 61)
    this.state = x
    return (x ^ (x >>> 14)) >>> 0
  }

  nextFloat() {
    return this.next() / 0xffffffff
  }

  range(min: number, max: number) {
    return min + this.nextFloat() * (max - min)
  }

  pick<T>(values: readonly T[]) {
    if (!values.length) {
      throw new Error('Cannot pick from empty list')
    }
    const index = Math.floor(this.nextFloat() * values.length)
    return values[index]
  }
}


