import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideHttpClient } from '@angular/common/http';

// const platform = platformBrowserDynamic();
// platform.bootstrapModule(AppModule);
platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.error(err));
// bootstrapApplication(AppComponent, {providers: [provideHttpClient()],

// }).catch(err => console.error(err));