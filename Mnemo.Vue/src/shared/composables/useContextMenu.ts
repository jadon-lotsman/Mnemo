import { nextTick, ref } from 'vue'
import { useActiveInput } from '@/shared/composables/useActiveInput.ts'
import { useSelection } from '@/shared/composables/useSelection.ts'
import { useEventListener } from '@vueuse/core'
import type { MenuConfig } from '@/features/contextMenu/types/MenuConfig'

const MENU_ELEMENT_OFFSET = 6
const MENU_MOUSE_OFFSET = 11
const MENU_FADE_DELAY = 120

const menuX = ref<number>(0)
const menuY = ref<number>(0)
const isVisible = ref<boolean>(false)
const isPositioned = ref(false)
const hasTriangle = ref<boolean>(true)
const isLeftAligned = ref<boolean>(false)
const isTopAligned = ref<boolean>(false)

const menuModules = ref<MenuConfig>()
const menuRef = ref<HTMLElement | null>(null)

interface OpenAt {
  clientX: number
  clientY: number
  modules: MenuConfig
  needTriangle: boolean
}

function getViewport() {
  const vv = window.visualViewport
  if (vv) {
    return {
      pageLeft: vv.pageLeft,
      pageTop: vv.pageTop,
      width: vv.width,
      height: vv.height,
    }
  }
  return {
    pageLeft: window.scrollX,
    pageTop: window.scrollY,
    width: window.innerWidth,
    height: window.innerHeight,
  }
}

export function useContextMenu() {
  const { hasActiveInput } = useActiveInput()
  const { hasSelection } = useSelection()

  async function openContextByElement(element: HTMLElement | null, modules: MenuConfig) {
    if (!element) return

    const rect = element.getBoundingClientRect()

    await openAt({
      clientX: rect.left,
      clientY: rect.bottom + MENU_ELEMENT_OFFSET,
      modules,
      needTriangle: false,
    })
  }

  async function openContextByMouse(event: MouseEvent, modules: MenuConfig, needTriangle = true) {
    if (hasActiveInput.value || hasSelection.value) return
    event.preventDefault()
    event.stopPropagation()

    await openAt({
      clientX: event.clientX,
      clientY: event.clientY,
      modules,
      needTriangle,
    })
  }

  async function openAt({ clientX, clientY, modules, needTriangle }: OpenAt) {
    const wasOpened = isVisible.value
    isVisible.value = false
    isPositioned.value = false

    menuModules.value = modules
    hasTriangle.value = needTriangle

    await new Promise((r) => setTimeout(r, wasOpened ? MENU_FADE_DELAY : 0))
    isVisible.value = true
    await nextTick()

    const el = menuRef.value
    if (!el) return

    const MENU_WIDTH = el.offsetWidth
    const MENU_HEIGHT = el.offsetHeight

    const vp = getViewport()

    const horizOffset = needTriangle ? MENU_MOUSE_OFFSET : 0

    const isFitsRight = clientX + MENU_WIDTH + horizOffset <= vp.width
    const isFitsBottom = clientY + MENU_HEIGHT <= vp.height

    let clientMenuX = clientX + (isFitsRight ? horizOffset : -MENU_WIDTH - horizOffset)
    let clientMenuY = clientY + (isFitsBottom ? 0 : -MENU_HEIGHT)

    clientMenuX = Math.max(0, Math.min(vp.width - MENU_WIDTH, clientMenuX))
    clientMenuY = Math.max(0, Math.min(vp.height - MENU_HEIGHT, clientMenuY))

    menuX.value = clientMenuX + vp.pageLeft
    menuY.value = clientMenuY + vp.pageTop
    isLeftAligned.value = !isFitsRight
    isTopAligned.value = !isFitsBottom
    isPositioned.value = true
  }

  function closeMenu() {
    if (isVisible.value) {
      isVisible.value = false
      isPositioned.value = false
      menuModules.value = undefined
    }
  }

  useEventListener(window, 'click', handleOutsideClick)
  useEventListener(window, 'contextmenu', handleOutsideClick)
  useEventListener(window, 'keydown', handleEscape)
  useEventListener(window, 'resize', closeMenu)
  useEventListener(window, 'scroll', closeMenu)

  function handleOutsideClick(event: MouseEvent) {
    const menu = document.querySelector('.context-menu')
    if (menu && !menu.contains(event.target as Node)) closeMenu()
  }

  function handleEscape(event: KeyboardEvent) {
    if (event.key === 'Escape') closeMenu()
  }

  return {
    menuX,
    menuY,
    menuModules,
    menuRef,
    isVisible,
    isPositioned,
    hasTriangle,
    isLeftAligned,
    isTopAligned,
    openContextByMouse,
    openContextByElement,
    closeMenu,
  }
}
