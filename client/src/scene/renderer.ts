import {
  AmbientLight,
  Color,
  DirectionalLight,
  FogExp2,
  GridHelper,
  HemisphereLight,
  Mesh,
  MeshBasicMaterial,
  MeshStandardMaterial,
  OrthographicCamera,
  PlaneGeometry,
  Scene,
  SpotLight,
  Vector3,
  WebGLRenderer
} from 'three'
import type { Material } from 'three'

export const createScene = (canvas: HTMLCanvasElement) => {
  const renderer = new WebGLRenderer({
    canvas,
    antialias: true
  })

  const scene = new Scene()
  scene.background = new Color(0x051c0f)
  scene.fog = new FogExp2(0x0a2c14, 0.05)

  const camera = new OrthographicCamera(-8, 8, 6.5, -6.5, 0.1, 100)
  camera.position.set(0, 3.5, 13.5)
  camera.lookAt(0, 0.5, 0)

  const hemisphere = new HemisphereLight(0x6d4c70, 0x0d050d, 0.7)
  scene.add(hemisphere)

  const keyLight = new DirectionalLight(0xff6b4a, 1.1)
  keyLight.position.set(5, 7, 5)
  keyLight.castShadow = false
  scene.add(keyLight)

  const ambient = new AmbientLight(0x14040a, 0.35)
  scene.add(ambient)

  const rimLight = new SpotLight(0xff4f6d, 0.8, 30, Math.PI / 6, 0.4)
  rimLight.position.set(-6, 6, -2)
  rimLight.target.position.set(0, 0.5, 0)
  scene.add(rimLight)
  scene.add(rimLight.target)

  const fillLight = new DirectionalLight(0x3d7fbf, 0.45)
  fillLight.position.set(-3, 4, 6)
  fillLight.castShadow = false
  scene.add(fillLight)

  const ground = new Mesh(
    new PlaneGeometry(60, 60),
    new MeshStandardMaterial({ color: 0x0f4e1a, roughness: 0.9 })
  )
  ground.rotation.x = -Math.PI / 2
  ground.position.y = -1.5
  scene.add(ground)

  const grid = new GridHelper(20, 20, 0x301a1c, 0x12080b)
  grid.position.y = -1.48
  const gridMaterial = grid.material as Material
  if ('transparent' in gridMaterial) {
    gridMaterial.transparent = true
    ;(gridMaterial as { opacity: number }).opacity = 0.25
  }
  scene.add(grid)

  const laneGeometry = new PlaneGeometry(12, 0.45)
  const laneMaterial = new MeshBasicMaterial({
    color: 0x2b0f15,
    transparent: true,
    opacity: 0.5
  })
  const laneOffsets = [-0.5, 0.2, 0.9]
  laneOffsets.forEach((offset, index) => {
    const lane = new Mesh(laneGeometry, laneMaterial.clone())
    lane.rotation.x = -Math.PI / 2
    lane.position.set(index === 0 ? -0.4 : 0.2, -1.47, offset)
    scene.add(lane)
  })

  const lookTarget = new Vector3(0, 0.5, 0)

  const resize = () => {
    const width = canvas.clientWidth || window.innerWidth
    const height = canvas.clientHeight || window.innerHeight
    const aspect = width / height
    const viewHeight = 6.5

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

  const updateCameraFocus = (x: number) => {
    const clampedX = Math.min(3.5, Math.max(-3.5, x))
    camera.position.x = clampedX
    lookTarget.set(clampedX, 0.5, 0)
    camera.lookAt(lookTarget)
  }

  const dispose = () => {
    window.removeEventListener('resize', resize)
  }

  return { renderer, scene, camera, dispose, updateCameraFocus }
}

