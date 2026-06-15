import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '10s', target: 50 },
    { duration: '30s', target: 100 },
    { duration: '10s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
  },
};

export function setup() {
  const loginUrl = 'http://localhost/api/auth/login';
  const payload = JSON.stringify({
    username: 'admin',
    password: 'admin123',
  });
  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };
  const res = http.post(loginUrl, payload, params);
  const token = res.json().token;
  return { token: token };
}

export default function (data) {
  const rand = Math.random();
  const randId = Math.floor(Math.random() * 200) + 1;
  
  if (rand < 0.1) {
    const uniqueId = __VU + '_' + __ITER + '_' + Math.floor(Math.random() * 10000);
    const username = 'user_' + uniqueId;
    const email = username + '@test.com';
    const password = 'Password123!';
    
    const regRes = http.post('http://localhost/api/auth/register', JSON.stringify({
      username: username,
      email: email,
      password: password
    }), { headers: { 'Content-Type': 'application/json' } });
    
    check(regRes, {
      'register status is 200': (r) => r.status === 200,
    });
    
    if (regRes.status === 200) {
      const userToken = regRes.json().token;
      const reviewPayload = JSON.stringify({
        text: 'Skvela kniha, urcite odporucam ' + uniqueId,
        rating: Math.floor(Math.random() * 5) + 1,
        bookId: randId,
      });
      const revRes = http.post('http://localhost/api/Review', reviewPayload, {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${userToken}`
        }
      });
      check(revRes, {
        'review status is 201': (r) => r.status === 201,
      });
    }
  } else if (rand < 0.4) {
    const sorts = ['popularity', 'rating', 'positivereviews', 'negativereviews', 'published', 'pages', 'alphabetical'];
    const sort = sorts[Math.floor(Math.random() * sorts.length)];
    const page = Math.floor(Math.random() * 5) + 1;
    const res = http.get(`http://localhost/api/Book?page=${page}&pageSize=20&sortBy=${sort}`, {
      headers: { 'Authorization': `Bearer ${data.token}` }
    });
    check(res, {
      'catalog status is 200': (r) => r.status === 200,
    });
  } else if (rand < 0.6) {
    const searches = ['a', 'b', 'c', 'the', 'book', 'history'];
    const search = searches[Math.floor(Math.random() * searches.length)];
    const sorts = ['popularity', 'rating', 'published', 'alphabetical'];
    const sort = sorts[Math.floor(Math.random() * sorts.length)];
    const res = http.get(`http://localhost/api/Book?page=1&pageSize=20&search=${search}&sortBy=${sort}`, {
      headers: { 'Authorization': `Bearer ${data.token}` }
    });
    check(res, {
      'search status is 200': (r) => r.status === 200,
    });
  } else if (rand < 0.8) {
    const res = http.get(`http://localhost/api/Book/${randId}`, {
      headers: { 'Authorization': `Bearer ${data.token}` }
    });
    check(res, {
      'details status is 200': (r) => r.status === 200,
    });
  } else if (rand < 0.9) {
    const endpoints = [
      'http://localhost/api/Genre',
      'http://localhost/api/Country',
      'http://localhost/api/Localization?lang=SK',
      'http://localhost/api/Localization?lang=EN',
      'http://localhost/api/Localization?lang=GR',
      'http://localhost/api/Author',
      'http://localhost/api/Reviewer'
    ];
    const endpoint = endpoints[Math.floor(Math.random() * endpoints.length)];
    const res = http.get(endpoint, {
      headers: { 'Authorization': `Bearer ${data.token}` }
    });
    check(res, {
      'meta status is 200': (r) => r.status === 200,
    });
  } else {
    const fileContent = 'dummy blob content ' + Math.random();
    const file = http.file(fileContent, 'dummy.png', 'image/png');
    const uploadRes = http.post('http://localhost/api/Blobs/upload', { file: file }, {
      headers: { 'Authorization': `Bearer ${data.token}` }
    });
    check(uploadRes, {
      'upload status is 200': (r) => r.status === 200,
    });
    
    if (uploadRes.status === 200) {
      const fileId = uploadRes.json().fileId;
      const getRes = http.get(`http://localhost/api/Blobs/${fileId}`, {
        headers: { 'Authorization': `Bearer ${data.token}` }
      });
      check(getRes, {
        'get blob status is 200': (r) => r.status === 200,
      });
      
      const delRes = http.del(`http://localhost/api/Blobs/${fileId}`, null, {
        headers: { 'Authorization': `Bearer ${data.token}` }
      });
      check(delRes, {
        'delete blob status is 200': (r) => r.status === 200,
      });
    }
  }
  
  sleep(0.1);
}
