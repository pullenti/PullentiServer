
import http.client
import unittest
import xml.etree.ElementTree as ET


text = '''
Единственным конкурентом «Трансмаша» на этом дебильном тендере было ООО «Плассер Алека Рейл Сервис», основным владельцем которого является австрийская компания «СТЦ-Холдинг ГМБХ». До конца 2011 г. эта же фирма была совладельцем «Трансмаша» вместе с «Тако» Краснова. Зато совладельцем «Плассера», также до конца 2011 г., был тот самый Карл Контрус, который имеет четверть акций «Трансмаша».
'''


class Test(unittest.TestCase):
    def setUp(self):
        # Run "make run" before
        self.conn = http.client.HTTPConnection('localhost', 8080)

    def tearDown(self):
        self.conn.close()

    def assertError(self, response):
        self.assertEqual(response.status, 400)
        data = response.read()
        xml = ET.fromstring(data)
        self.assertEqual(xml.tag, 'error')

    def assertResult(self, response):
        self.assertEqual(response.status, 200)
        data = response.read()
        xml = ET.fromstring(data)
        self.assertEqual(xml.tag, 'result')

    def test_bad_method(self):
        self.conn.request('GET', '/')
        self.assertError(self.conn.getresponse())

    def test_no_data(self):
        self.conn.request('POST', '/', '')
        self.assertError(self.conn.getresponse())

    def test_ok(self):
        self.conn.request('POST', '/', text.encode('utf8'))
        self.assertResult(self.conn.getresponse())


if __name__ == '__main__':
    unittest.main()
