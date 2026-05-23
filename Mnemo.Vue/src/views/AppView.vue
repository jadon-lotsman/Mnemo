<script setup lang="ts">
import CollapsibleSection from '@/common/CollapsibleSection.vue'
import Calendar from '@/features/calendar/components/CalendarComponent.vue'
import Launcher from '@/features/launcher/components/LauncherComponent.vue'
import Vocabulary from '@/features/vocabulary/components/VocabularyComponent.vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const logout = () => {
  localStorage.removeItem('token')
  router.push('/login')
}
</script>

<template>
  <main>
    <CollapsibleSection title="Calendar">
      <Calendar></Calendar>
    </CollapsibleSection>

    <CollapsibleSection title="Launcher" :initial-open="false">
      <Launcher></Launcher>
    </CollapsibleSection>

    <CollapsibleSection title="Vocabulary">
      <template #subtext>
        <span>some subtext</span>
      </template>

      <Vocabulary></Vocabulary>
    </CollapsibleSection>

    <button @click="logout">Logout</button>
  </main>
</template>
