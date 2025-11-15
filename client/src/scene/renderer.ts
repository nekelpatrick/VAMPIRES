import {
  Color,
  DirectionalLight,
  FogExp2,
  HemisphereLight,
  Mesh,
  MeshStandardMaterial,
  OrthographicCamera,
  PlaneGeometry,
  Scene,
  WebGLRenderer
} from 'three'

export const createScene = (canvas: HTMLCanvasElement) => {
  const renderer = new WebGLRenderer({
    canvas,
    antialias: true
  })

  const scene = new Scene()
  scene.background = new Color(0x050308)
  scene.fog = new FogExp2(0x050308, 0.08)

  const camera = new OrthographicCamera(-6, 6, 4, -4, 0.1, 100)
  camera.position.set(0, 2, 10)
  camera.lookAt(0, 1, 0)

  const hemisphere = new HemisphereLight(0x6d4c70, 0x0d050d, 0.9)
  scene.add(hemisphere)

  const directional = new DirectionalLight(0xff6b4a, 1.4)
  directional.position.set(5, 7, 5)
  directional.castShadow = false
  scene.add(directional)

  const ground = new Mesh(
    new PlaneGeometry(60, 60),
    new MeshStandardMaterial({ color: 0x1a090f, roughness: 1 })
  )
  ground.rotation.x = -Math.PI / 2
  ground.position.y = -1.5
  scene.add(ground)

  const resize = () => {
    const width = canvas.clientWidth || window.innerWidth
    const height = canvas.clientHeight || window.innerHeight
    const aspect = width / height
    const viewHeight = 4

    camera.left = -viewHeight * aspect
    camera.right = viewHeight * aspect
    camera.top = viewHeight
    camera.bottom = -viewHeight
    camera.updateProjectionMatrix()

    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))
    renderer.setSize(width, height, false)
  }

  resize()
  window.addEventListener('resize', resize)

  const dispose = () => {
    window.removeEventListener('resize', resize)
  }

  return { renderer, scene, camera, dispose }
}

