const rhinocommonProps = {
  apiTitle: 'RhinoCommon',
  baseUrl: '/rhinocommon/'
}

const routes = [
  {
    path: '/rhinocommon/',
    component: () => import('layouts/MainLayout.vue'),
    props: rhinocommonProps,
    children: [
      { path: '', component: () => import('pages/Index.vue'), props: rhinocommonProps },
      { path: 'examples/:example', component: () => import('pages/Example.vue') },
      { path: 'whatsnew/:version', component: () => import('pages/WhatsNew.vue'), props: rhinocommonProps },
      { path: ':datatype', component: () => import('pages/DataType.vue'), props: rhinocommonProps }
    ]
  }
]

// Always leave this as last one
if (process.env.MODE !== 'ssr') {
  routes.push({
    path: '*',
    component: () => import('pages/Error404.vue')
  })
}

export default routes
