import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useAudioStore = defineStore('audio', () => {
  const currentAudio = ref<HTMLAudioElement | null>(null)
  const currentUrl = ref<string | null>(null)

  function play(url: string) {
    if (currentAudio.value) {
      currentAudio.value.pause()
      currentAudio.value.currentTime = 0
    }

    const audio = new Audio(url)
    audio.play()
    currentAudio.value = audio
    currentUrl.value = url

    audio.onended = () => {
      if (currentAudio.value === audio) {
        currentAudio.value = null
        currentUrl.value = null
      }
    }
  }

  function isPlayingThis(url: string) {
    return currentUrl.value === url && currentAudio.value !== null && !currentAudio.value.paused
  }

  function stop() {
    if (currentAudio.value) {
      currentAudio.value.pause()
      currentAudio.value.currentTime = 0
      currentAudio.value = null
      currentUrl.value = null
    }
  }

  return {
    play,
    stop,
    isPlayingThis,
  }
})
