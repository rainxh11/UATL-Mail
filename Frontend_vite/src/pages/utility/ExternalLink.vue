<template>
  <div>
    <v-img
      class="mb-5"
      max-height="300"
      contain
      src="/images/illustrations/external-link-warning.svg"
    ></v-img>

    <h1 class="display-1 font-weight-bold">{{ $t('utility.externalLink') }}</h1>
    <a :href="$route.query.link" class="title mt-d4">
      {{ $route.query.link }}
    </a>
  </div>
</template>
