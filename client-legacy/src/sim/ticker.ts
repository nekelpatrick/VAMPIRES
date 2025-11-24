type TickHandler = (deltaSeconds: number) => void

export class FixedTicker {
  private accumulator = 0
  private readonly stepSeconds: number
  private readonly handler: TickHandler

  constructor(stepMilliseconds: number, handler: TickHandler) {
    this.stepSeconds = stepMilliseconds / 1000
    this.handler = handler
  }

  update(deltaSeconds: number) {
    this.accumulator += deltaSeconds

    while (this.accumulator >= this.stepSeconds) {
      this.handler(this.stepSeconds)
      this.accumulator -= this.stepSeconds
    }
  }
}


