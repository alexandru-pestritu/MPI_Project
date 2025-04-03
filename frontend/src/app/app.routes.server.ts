import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: '**',
    renderMode: RenderMode.Prerender
  },
  {
    path: 'reset-password/:token',
    renderMode: RenderMode.Server,
  },
  {
    path: 'course/:courseId',
    renderMode: RenderMode.Server,
  },
];
